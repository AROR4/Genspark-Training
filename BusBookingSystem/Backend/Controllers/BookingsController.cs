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
    private readonly CancellationService _cancellationService;
    private readonly PaymentService _paymentService;

    public BookingsController(
        AppDbContext dbContext,
        TicketService ticketService,
        BookingEmailService bookingEmailService,
        CancellationService cancellationService,
        PaymentService paymentService)
    {
        _dbContext = dbContext;
        _ticketService = ticketService;
        _bookingEmailService = bookingEmailService;
        _cancellationService = cancellationService;
        _paymentService = paymentService;
    }

    [AllowAnonymous]
    [HttpGet("search")]
    public async Task<ActionResult<List<ScheduleSearchResponse>>> SearchSchedules(
        [FromQuery] int sourceCityId,
        [FromQuery] int destinationCityId,
        [FromQuery] DateOnly travelDate)
    {
        if (sourceCityId <= 0 || destinationCityId <= 0)
        {
            return BadRequest(new { message = "Source and destination city IDs must be valid." });
        }

        if (sourceCityId == destinationCityId)
        {
            return BadRequest(new { message = "Source and destination city IDs must be different." });
        }

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
                schedule.Route.SourceCityId == sourceCityId &&
                schedule.Route.DestinationCityId == destinationCityId)
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
    [HttpGet("/api/seats/{scheduleId:int}")]
    [HttpGet("schedules/{scheduleId:int}/seats")]
    public async Task<ActionResult<List<SeatAvailabilityResponse>>> GetAvailableSeats(int scheduleId)
    {
        var schedule = await _dbContext.BusSchedules
            .AsNoTracking()
            .Include(busSchedule => busSchedule.Bus)
                .ThenInclude(bus => bus!.Seats)
            .Include(busSchedule => busSchedule.SeatAvailabilities)
                .ThenInclude(seatAvailability => seatAvailability.Seat)
            .FirstOrDefaultAsync(busSchedule => busSchedule.Id == scheduleId);

        if (schedule?.Bus is null)
        {
            return NotFound(new { message = "Schedule was not found." });
        }

        if (schedule.IsCancelled || !schedule.Bus.IsActive || schedule.Route?.IsActive == false)
        {
            return BadRequest(new { message = "Schedule is not available for booking." });
        }

        var now = DateTime.UtcNow;
        var seatAvailabilityLookup = schedule.SeatAvailabilities
            .ToDictionary(seatAvailability => seatAvailability.SeatId, seatAvailability => seatAvailability);

        return Ok(schedule.Bus.Seats
            .OrderBy(seat => seat.SeatNumber)
            .Select(seat =>
            {
                var availability = seatAvailabilityLookup.GetValueOrDefault(seat.Id);
                var isExpiredHold = availability?.Status == "Held" &&
                    availability.HoldExpiry.HasValue &&
                    availability.HoldExpiry.Value <= now;

                return new SeatAvailabilityResponse(
                    availability?.Id ?? 0,
                    seat.SeatNumber ?? string.Empty,
                    isExpiredHold ? "Available" : availability?.Status ?? "Available",
                    isExpiredHold ? null : availability?.HoldExpiry);
            })
            .ToList());
    }

    [HttpPost]
    [HttpPost("/api/booking/hold")]
    public async Task<IActionResult> CreateBooking(CreateBookingRequest request, CancellationToken cancellationToken)
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

        if (schedule.IsCancelled || !schedule.Bus.IsActive || schedule.Route?.IsActive == false)
        {
            return BadRequest(new { message = "Schedule is not available for booking." });
        }

        var seatAvailabilityIds = request.Passengers.Select(passenger => passenger.SeatAvailabilityId).ToList();
        if (seatAvailabilityIds.Count != seatAvailabilityIds.Distinct().Count())
        {
            return BadRequest(new { message = "A seat cannot be selected more than once in the same booking." });
        }

        var seatAvailabilities = await _dbContext.SeatAvailabilities
            .Include(seatAvailability => seatAvailability.Seat)
            .Where(seatAvailability => seatAvailability.BusScheduleId == schedule.Id && seatAvailabilityIds.Contains(seatAvailability.Id))
            .OrderBy(seatAvailability => seatAvailability.Seat.SeatNumber)
            .ToListAsync(cancellationToken);

        if (seatAvailabilities.Count != seatAvailabilityIds.Count)
        {
            return BadRequest(new { message = "One or more selected seats are invalid for this bus." });
        }

        var now = DateTime.UtcNow;
        var unavailableSeat = seatAvailabilities.FirstOrDefault(seatAvailability => IsSeatUnavailable(seatAvailability, now));
        if (unavailableSeat is not null)
        {
            return Conflict(new { message = $"Seat {unavailableSeat.Seat?.SeatNumber} is already reserved." });
        }

        var priceBreakdown = _paymentService.CalculateBookingAmount(schedule.BasePrice, request.Passengers.Count);

        var booking = new Booking
        {
            UserId = user.Id,
            ScheduleId = schedule.Id,
            BaseAmount = priceBreakdown.BaseAmount,
            GstAmount = priceBreakdown.GstAmount,
            ConvenienceFee = priceBreakdown.ConvenienceFee,
            TotalAmount = priceBreakdown.TotalAmount,
            Status = "PENDING",
            ContactEmail = request.ContactEmail?.Trim(),
            ContactPhone = request.ContactPhone?.Trim()
        };

        foreach (var passenger in request.Passengers)
        {
            var seatAvailability = seatAvailabilities.First(seat => seat.Id == passenger.SeatAvailabilityId);
            seatAvailability.Status = "Held";
            seatAvailability.HoldExpiry = now.AddMinutes(_paymentService.GetSeatHoldMinutes());

            booking.BookingPassengers.Add(new BookingPassenger
            {
                Name = passenger.Name.Trim(),
                Age = passenger.Age,
                Gender = passenger.Gender.Trim().ToUpperInvariant(),
                SeatAvailability = seatAvailability
            });
        }

        _dbContext.Bookings.Add(booking);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Ok(new { booking.Id });
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
                .ThenInclude(passenger => passenger.SeatAvailability)
                    .ThenInclude(seatAvailability => seatAvailability!.Seat)
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

    [HttpPost("{bookingId:int}/cancel")]
    public async Task<ActionResult<CancelBookingResponse>> CancelTicket(int bookingId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _cancellationService.CancelUserBookingAsync(bookingId, userId.Value, cancellationToken);
        if (result is null)
        {
            return NotFound(new { message = "Booking was not found." });
        }

        if (result.Value.Booking.Status is "FAILED" or "EXPIRED")
        {
            return BadRequest(new { message = "This booking cannot be cancelled." });
        }

        if (!result.Value.CancelledNow)
        {
            return BadRequest(new { message = "Booking is already cancelled." });
        }

        return Ok(new CancelBookingResponse(
            "Ticket cancelled successfully.",
            result.Value.RefundPercentage,
            result.Value.RefundAmount,
            ToBookingResponse(result.Value.Booking, result.Value.EmailSent)));
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
        var now = DateTime.UtcNow;

        var seatIds = await _dbContext.SeatAvailabilities
            .AsNoTracking()
            .Where(seatAvailability =>
                seatAvailability.BusScheduleId == scheduleId &&
                seatAvailability.Status != "Available" &&
                (seatAvailability.Status != "Held" || !seatAvailability.HoldExpiry.HasValue || seatAvailability.HoldExpiry > now))
            .Select(seatAvailability => seatAvailability.SeatId)
            .ToListAsync(cancellationToken);

        return seatIds.ToHashSet();
    }

    private async Task<Dictionary<int, int>> GetBookedSeatCountsAsync(List<int> scheduleIds)
    {
        var now = DateTime.UtcNow;

        return await _dbContext.SeatAvailabilities
            .AsNoTracking()
            .Where(passenger =>
                scheduleIds.Contains(passenger.BusScheduleId) &&
                passenger.Status != "Available" &&
                (passenger.Status != "Held" || !passenger.HoldExpiry.HasValue || passenger.HoldExpiry > now))
            .GroupBy(passenger => passenger.BusScheduleId)
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
                .ThenInclude(passenger => passenger.SeatAvailability)
                    .ThenInclude(seatAvailability => seatAvailability!.Seat)
            .Include(booking => booking.Payments)
            .FirstOrDefaultAsync(booking => booking.Id == bookingId && booking.UserId == userId, cancellationToken);
    }

    private static bool IsSeatUnavailable(SeatAvailability? seatAvailability, DateTime now)
    {
        if (seatAvailability is null)
        {
            return false;
        }

        if (seatAvailability.Status == "Available")
        {
            return false;
        }

        if (seatAvailability.Status == "Held" && seatAvailability.HoldExpiry.HasValue && seatAvailability.HoldExpiry <= now)
        {
            return false;
        }

        return true;
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
            booking.BaseAmount,
            booking.GstAmount,
            booking.ConvenienceFee,
            booking.TotalAmount,
            paymentStatus,
            booking.CreatedAt,
            booking.ContactEmail ?? booking.User?.Email,
            booking.ContactPhone ?? booking.User?.PhoneNumber,
            booking.RefundAmount,
            booking.OperatorLoss,
            booking.AdminRevenue,
            booking.CancellationType,
            new BookingJourneyResponse(
                schedule.Id,
                schedule.BusId ?? 0,
                schedule.Bus?.RegistrationNumber ?? string.Empty,
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
                .OrderBy(passenger => passenger.SeatAvailability!.Seat.SeatNumber)
                .Select(passenger => new TicketPassengerResponse(
                    passenger.Name ?? string.Empty,
                    passenger.Age ?? 0,
                    passenger.Gender ?? string.Empty,
                    passenger.SeatAvailability?.Seat.SeatNumber ?? string.Empty))
                .ToList(),
            emailSent);
    }
}
