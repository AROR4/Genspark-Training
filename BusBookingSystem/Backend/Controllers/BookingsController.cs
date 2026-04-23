using System.Data;
using System.Security.Claims;
using BusBookingApp.Data;
using BusBookingApp.Dtos;
using BusBookingApp.Models;
using BusBookingApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusBookingApp.Controllers;

[ApiController]
[Authorize(Roles = "USER")]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly TicketService _ticketService;
    private readonly BookingEmailService _bookingEmailService;
    private readonly PaymentService _paymentService;

    public BookingsController(
        AppDbContext dbContext,
        TicketService ticketService,
        BookingEmailService bookingEmailService,
        PaymentService paymentService)
    {
        _dbContext = dbContext;
        _ticketService = ticketService;
        _bookingEmailService = bookingEmailService;
        _paymentService = paymentService;
    }

    [AllowAnonymous]
    [HttpGet("search")]
    public async Task<ActionResult<List<ScheduleSearchResponse>>> SearchSchedules(
        [FromQuery] string sourceCityName,
        [FromQuery] string destinationCityName,
        [FromQuery] DateOnly travelDate)
    {
        var source = sourceCityName.Trim().ToLowerInvariant();
        var destination = destinationCityName.Trim().ToLowerInvariant();

        var schedules = await _dbContext.BusSchedules
            .AsNoTracking()
            .Include(schedule => schedule.Bus)
                .ThenInclude(bus => bus!.Operator)
            .Include(schedule => schedule.Route)
                .ThenInclude(route => route!.SourceCity)
            .Include(schedule => schedule.Route)
                .ThenInclude(route => route!.DestinationCity)
            .Where(schedule =>
                schedule.TravelDate == travelDate &&
                schedule.Bus != null &&
                schedule.Bus.IsActive &&
                schedule.Route != null &&
                schedule.Route.SourceCity != null &&
                schedule.Route.DestinationCity != null &&
                schedule.Route.SourceCity.Name.ToLower() == source &&
                schedule.Route.DestinationCity.Name.ToLower() == destination)
            .OrderBy(schedule => schedule.DepartureTime)
            .ToListAsync();

        var scheduleIds = schedules.Select(schedule => schedule.Id).ToList();
        var bookedSeatCounts = await GetBookedSeatCountsAsync(scheduleIds);

        return Ok(schedules.Select(schedule =>
        {
            var totalSeats = schedule.Bus!.TotalSeats;
            var bookedSeats = bookedSeatCounts.GetValueOrDefault(schedule.Id);

            return new ScheduleSearchResponse(
                schedule.Id,
                schedule.BusId ?? 0,
                schedule.Bus.Operator?.CompanyName,
                schedule.Route!.SourceCity!.Name,
                schedule.Route.DestinationCity!.Name,
                schedule.TravelDate,
                schedule.DepartureTime,
                schedule.ArrivalDate,
                schedule.ArrivalTime,
                schedule.DurationMinutes,
                schedule.BasePrice,
                totalSeats,
                totalSeats - bookedSeats);
        }).ToList());
    }

    [AllowAnonymous]
    [HttpGet("schedules/{scheduleId:int}/seats")]
    public async Task<ActionResult<List<SeatAvailabilityResponse>>> GetAvailableSeats(int scheduleId)
    {
        var schedule = await _dbContext.BusSchedules
            .AsNoTracking()
            .Include(busSchedule => busSchedule.Bus)
                .ThenInclude(bus => bus!.Seats)
            .FirstOrDefaultAsync(busSchedule => busSchedule.Id == scheduleId);

        if (schedule?.Bus is null)
        {
            return NotFound(new { message = "Schedule was not found." });
        }

        var bookedSeatIds = await GetBookedSeatIdsAsync(scheduleId);

        return Ok(schedule.Bus.Seats
            .OrderBy(seat => seat.SeatNumber)
            .Select(seat => new SeatAvailabilityResponse(
                seat.Id,
                seat.SeatNumber ?? string.Empty,
                bookedSeatIds.Contains(seat.Id)))
            .ToList());
    }

    [HttpPost]
    public async Task<ActionResult<BookingResponse>> CreateBooking(CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(currentUser => currentUser.Id == userId.Value, cancellationToken);
        if (user is null)
        {
            return Unauthorized();
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var schedule = await _dbContext.BusSchedules
            .Include(busSchedule => busSchedule.Bus)
                .ThenInclude(bus => bus!.Operator)
            .Include(busSchedule => busSchedule.Route)
                .ThenInclude(route => route!.SourceCity)
            .Include(busSchedule => busSchedule.Route)
                .ThenInclude(route => route!.DestinationCity)
            .FirstOrDefaultAsync(busSchedule => busSchedule.Id == request.ScheduleId, cancellationToken);

        if (schedule?.Bus is null || schedule.Route?.SourceCity is null || schedule.Route.DestinationCity is null)
        {
            return NotFound(new { message = "Schedule was not found." });
        }

        var seatIds = request.Passengers.Select(passenger => passenger.SeatId).ToList();
        if (seatIds.Count != seatIds.Distinct().Count())
        {
            return BadRequest(new { message = "A seat cannot be selected more than once in the same booking." });
        }

        var seats = await _dbContext.Seats
            .Where(seat => seat.BusId == schedule.BusId && seatIds.Contains(seat.Id))
            .OrderBy(seat => seat.SeatNumber)
            .ToListAsync(cancellationToken);

        if (seats.Count != seatIds.Count)
        {
            return BadRequest(new { message = "One or more selected seats are invalid for this bus." });
        }

        var bookedSeatIds = await GetBookedSeatIdsAsync(schedule.Id, cancellationToken);
        var unavailableSeat = seats.FirstOrDefault(seat => bookedSeatIds.Contains(seat.Id));
        if (unavailableSeat is not null)
        {
            return Conflict(new { message = $"Seat {unavailableSeat.SeatNumber} is already booked." });
        }

        var totalAmount = schedule.BasePrice * request.Passengers.Count;

        var booking = new Booking
        {
            UserId = user.Id,
            ScheduleId = schedule.Id,
            TotalAmount = totalAmount,
            Status = "CONFIRMED",
            ContactEmail = request.ContactEmail?.Trim(),
            ContactPhone = request.ContactPhone?.Trim()
        };

        var seatLookup = seats.ToDictionary(seat => seat.Id);
        foreach (var passenger in request.Passengers)
        {
            booking.BookingPassengers.Add(new BookingPassenger
            {
                Name = passenger.Name.Trim(),
                Age = passenger.Age,
                Gender = passenger.Gender.Trim().ToUpperInvariant(),
                SeatId = passenger.SeatId
            });
        }

        booking.Payments.Add(new Payment
        {
            Amount = totalAmount,
            Status = "SUCCESS",
            RefundStatus = "NONE"
        });

        _dbContext.Bookings.Add(booking);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var hydratedBooking = await LoadBookingAsync(booking.Id, user.Id, cancellationToken);
        if (hydratedBooking is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Booking was created but could not be loaded." });
        }

        var emailSent = false;
        try
        {
            emailSent = await _bookingEmailService.TrySendTicketAsync(hydratedBooking, cancellationToken);
        }
        catch
        {
            emailSent = false;
        }

        return Created($"/api/bookings/{hydratedBooking.Id}/ticket", ToBookingResponse(
            hydratedBooking,
            emailSent));
    }

    [HttpGet]
    public async Task<ActionResult<List<BookingResponse>>> GetMyBookings(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var bookings = await _dbContext.Bookings
            .AsNoTracking()
            .Include(booking => booking.Schedule)
                .ThenInclude(schedule => schedule!.Bus)
                    .ThenInclude(bus => bus!.Operator)
            .Include(booking => booking.Schedule)
                .ThenInclude(schedule => schedule!.Route)
                    .ThenInclude(route => route!.SourceCity)
            .Include(booking => booking.Schedule)
                .ThenInclude(schedule => schedule!.Route)
                    .ThenInclude(route => route!.DestinationCity)
            .Include(booking => booking.BookingPassengers)
                .ThenInclude(passenger => passenger.Seat)
            .Include(booking => booking.Payments)
            .Where(booking => booking.UserId == userId.Value)
            .OrderByDescending(booking => booking.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(bookings.Select(booking => ToBookingResponse(booking, false)).ToList());
    }

    [HttpGet("{bookingId:int}/ticket")]
    public async Task<ActionResult<BookingResponse>> ViewTicket(int bookingId, CancellationToken cancellationToken)
    {
        var booking = await GetUserBookingAsync(bookingId, cancellationToken);
        if (booking is null)
        {
            return NotFound(new { message = "Booking was not found." });
        }

        return Ok(ToBookingResponse(booking, false));
    }

    [HttpGet("{bookingId:int}/ticket/download")]
    public async Task<IActionResult> DownloadTicket(int bookingId, CancellationToken cancellationToken)
    {
        var booking = await GetUserBookingAsync(bookingId, cancellationToken);
        if (booking is null)
        {
            return NotFound(new { message = "Booking was not found." });
        }

        var bookingCode = _ticketService.GenerateBookingCode(booking);
        var ticketNumber = _ticketService.GenerateTicketNumber(booking);
        var content = _ticketService.BuildTicketContent(
            booking,
            bookingCode,
            ticketNumber,
            booking.Schedule!.Route!.SourceCity!.Name,
            booking.Schedule.Route.DestinationCity!.Name,
            booking.Schedule.Bus!.Operator?.CompanyName ?? "Bus Operator",
            booking.Payments.OrderByDescending(payment => payment.CreatedAt).FirstOrDefault()?.Status ?? "SUCCESS");

        return File(
            System.Text.Encoding.UTF8.GetBytes(content),
            "text/plain",
            $"{ticketNumber}.txt");
    }

    [HttpPost("{bookingId:int}/ticket/email")]
    public async Task<ActionResult<TicketEmailResponse>> EmailTicket(int bookingId, CancellationToken cancellationToken)
    {
        var booking = await GetUserBookingAsync(bookingId, cancellationToken);
        if (booking is null)
        {
            return NotFound(new { message = "Booking was not found." });
        }

        try
        {
            var sent = await _bookingEmailService.TrySendTicketAsync(booking, cancellationToken);
            var email = booking.User?.Email ?? string.Empty;

            return Ok(new TicketEmailResponse(
                booking.Id,
                email,
                sent,
                sent ? "Ticket email sent." : "SMTP is not configured, so email was skipped."));
        }
        catch (Exception exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new TicketEmailResponse(
                booking.Id,
                booking.User?.Email ?? string.Empty,
                false,
                $"Ticket email failed: {exception.Message}"));
        }
    }

    private int? GetUserId()
    {
        return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
            ? userId
            : null;
    }

    private async Task<HashSet<int>> GetBookedSeatIdsAsync(int scheduleId, CancellationToken cancellationToken = default)
    {
        var holdCutoffUtc = _paymentService.GetActiveSeatHoldCutoffUtc();

        var seatIds = await _dbContext.BookingPassengers
            .AsNoTracking()
            .Where(passenger =>
                passenger.Booking != null &&
                passenger.Booking.ScheduleId == scheduleId &&
                (passenger.Booking.Status == "CONFIRMED" ||
                 (passenger.Booking.Status == "PENDING" && passenger.Booking.CreatedAt >= holdCutoffUtc)) &&
                passenger.SeatId != null)
            .Select(passenger => passenger.SeatId!.Value)
            .ToListAsync(cancellationToken);

        return seatIds.ToHashSet();
    }

    private async Task<Dictionary<int, int>> GetBookedSeatCountsAsync(List<int> scheduleIds)
    {
        var holdCutoffUtc = _paymentService.GetActiveSeatHoldCutoffUtc();

        return await _dbContext.BookingPassengers
            .AsNoTracking()
            .Where(passenger =>
                passenger.Booking != null &&
                passenger.Booking.ScheduleId != null &&
                scheduleIds.Contains(passenger.Booking.ScheduleId.Value) &&
                (passenger.Booking.Status == "CONFIRMED" ||
                 (passenger.Booking.Status == "PENDING" && passenger.Booking.CreatedAt >= holdCutoffUtc)) &&
                passenger.SeatId != null)
            .GroupBy(passenger => passenger.Booking!.ScheduleId!.Value)
            .Select(group => new { ScheduleId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.ScheduleId, item => item.Count);
    }

    private async Task<Booking?> GetUserBookingAsync(int bookingId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return null;
        }

        return await LoadBookingAsync(bookingId, userId.Value, cancellationToken);
    }

    private async Task<Booking?> LoadBookingAsync(int bookingId, int userId, CancellationToken cancellationToken)
    {
        return await _dbContext.Bookings
            .AsNoTracking()
            .Include(booking => booking.User)
            .Include(booking => booking.Schedule)
                .ThenInclude(schedule => schedule!.Bus)
                    .ThenInclude(bus => bus!.Operator)
            .Include(booking => booking.Schedule)
                .ThenInclude(schedule => schedule!.Route)
                    .ThenInclude(route => route!.SourceCity)
            .Include(booking => booking.Schedule)
                .ThenInclude(schedule => schedule!.Route)
                    .ThenInclude(route => route!.DestinationCity)
            .Include(booking => booking.BookingPassengers)
                .ThenInclude(passenger => passenger.Seat)
            .Include(booking => booking.Payments)
            .FirstOrDefaultAsync(booking => booking.Id == bookingId && booking.UserId == userId, cancellationToken);
    }

    private BookingResponse ToBookingResponse(Booking booking, bool emailSent)
    {
        var schedule = booking.Schedule!;
        var paymentStatus = booking.Payments.OrderByDescending(payment => payment.CreatedAt).FirstOrDefault()?.Status ?? "SUCCESS";
        var bookingCode = _ticketService.GenerateBookingCode(booking);
        var ticketNumber = _ticketService.GenerateTicketNumber(booking);

        return new BookingResponse(
            booking.Id,
            bookingCode,
            ticketNumber,
            booking.Status ?? "CONFIRMED",
            booking.TotalAmount ?? 0,
            paymentStatus,
            booking.CreatedAt,
            booking.ContactEmail ?? booking.User?.Email,
            booking.ContactPhone ?? booking.User?.PhoneNumber,
            new BookingJourneyResponse(
                schedule.Id,
                schedule.BusId ?? 0,
                schedule.Bus?.Operator?.CompanyName,
                schedule.Route!.SourceCity!.Name,
                schedule.Route.DestinationCity!.Name,
                schedule.TravelDate,
                schedule.DepartureTime,
                schedule.ArrivalDate,
                schedule.ArrivalTime,
                schedule.DurationMinutes,
                schedule.BasePrice,
                schedule.Bus?.TotalSeats ?? 0),
            booking.BookingPassengers
                .OrderBy(passenger => passenger.Seat!.SeatNumber)
                .Select(passenger => new TicketPassengerResponse(
                    passenger.Name ?? string.Empty,
                    passenger.Age ?? 0,
                    passenger.Gender ?? string.Empty,
                    passenger.Seat?.SeatNumber ?? string.Empty))
                .ToList(),
            emailSent);
    }
}
