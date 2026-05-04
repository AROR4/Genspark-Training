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
public class PaymentsController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly PaymentService _paymentService;
    private readonly TicketService _ticketService;
    private readonly BookingEmailService _bookingEmailService;

    public PaymentsController(
        AppDbContext dbContext,
        PaymentService paymentService,
        TicketService ticketService,
        BookingEmailService bookingEmailService)
    {
        _dbContext = dbContext;
        _paymentService = paymentService;
        _ticketService = ticketService;
        _bookingEmailService = bookingEmailService;
    }

    [HttpPost("/api/payment/init")]
    public async Task<ActionResult<PaymentResponse>> InitPayment(InitPaymentRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var booking = await _dbContext.Bookings
            .Include(currentBooking => currentBooking.User)
            .Include(currentBooking => currentBooking.Schedule)
            .Include(currentBooking => currentBooking.Payments)
            .FirstOrDefaultAsync(currentBooking => currentBooking.Id == request.BookingId && currentBooking.UserId == userId.Value, cancellationToken);

        if (booking is null)
        {
            return NotFound(new { message = "Booking was not found." });
        }

        if (booking.Status == "CONFIRMED")
        {
            return BadRequest(new { message = "Confirmed booking cannot be reinitialized." });
        }

        var existingPayment = booking.Payments
            .OrderByDescending(payment => payment.CreatedAt)
            .FirstOrDefault(payment => payment.Status == "PENDING" || payment.Status == "INITIATED");

        if (existingPayment is not null)
        {
            return Ok(ToPaymentResponse(existingPayment));
        }

        var payment = new Payment
        {
            BookingId = booking.Id,
            Amount = booking.TotalAmount,
            PaymentMethod = "RAZORPAY",
            GatewayOrderId = _paymentService.GenerateGatewayOrderId(),
            Status = "PENDING",
            RefundStatus = "NONE"
        };

        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToPaymentResponse(payment));
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<PaymentCheckoutResponse>> CreateCheckout(CreateBookingRequest request, CancellationToken cancellationToken)
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
            .Include(busSchedule => busSchedule.Route)
                .ThenInclude(route => route!.SourceCity)
            .Include(busSchedule => busSchedule.Route)
                .ThenInclude(route => route!.DestinationCity)
            .Include(busSchedule => busSchedule.SeatAvailabilities)
                .ThenInclude(seatAvailability => seatAvailability.Seat)
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
            return BadRequest(new { message = "A seat cannot be selected more than once in the same payment request." });
        }

        var seatAvailabilities = await _dbContext.SeatAvailabilities
            .Include(seatAvailability => seatAvailability.Seat)
            .Where(seatAvailability => seatAvailability.BusScheduleId == schedule.Id && seatAvailabilityIds.Contains(seatAvailability.Id))
            .ToListAsync(cancellationToken);

        if (seatAvailabilities.Count != seatAvailabilityIds.Count)
        {
            return BadRequest(new { message = "One or more selected seats are invalid for this bus." });
        }

        var now = DateTime.UtcNow;
        var unavailableSeat = seatAvailabilities.FirstOrDefault(seatAvailability =>
            seatAvailability.Status == "Booked" ||
            (seatAvailability.Status == "Held" && (!seatAvailability.HoldExpiry.HasValue || seatAvailability.HoldExpiry > now)));
        if (unavailableSeat is not null)
        {
            return Conflict(new { message = $"Seat {unavailableSeat.Seat?.SeatNumber} is already reserved." });
        }

        var priceBreakdown = _paymentService.CalculateBookingAmount(schedule.BasePrice, request.Passengers.Count);
        var gatewayOrderId = _paymentService.GenerateGatewayOrderId();

        foreach (var seatAvailability in seatAvailabilities)
        {
            seatAvailability.Status = "Held";
            seatAvailability.HoldExpiry = _paymentService.GetExpiryUtc(now);
        }

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
            booking.BookingPassengers.Add(new BookingPassenger
            {
                Name = passenger.Name.Trim(),
                Age = passenger.Age,
                Gender = passenger.Gender.Trim().ToUpperInvariant(),
                SeatAvailability = seatAvailability
            });
        }

        var payment = new Payment
        {
            Amount = booking.TotalAmount,
            PaymentMethod = request.PaymentMethod.Trim().ToUpperInvariant(),
            PaymentReference = request.PaymentReference?.Trim(),
            GatewayOrderId = gatewayOrderId,
            Status = "PENDING",
            RefundStatus = "NONE"
        };
        booking.Payments.Add(payment);

        _dbContext.Bookings.Add(booking);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Created($"/api/payments/{payment.Id}", new PaymentCheckoutResponse(
            booking.Id,
            payment.Id,
            booking.Status ?? "PENDING",
            payment.Status ?? "PENDING",
            gatewayOrderId,
            payment.Amount ?? 0,
            payment.PaymentMethod ?? request.PaymentMethod.Trim().ToUpperInvariant(),
            _paymentService.GetExpiryUtc(booking.CreatedAt)));
    }

    [HttpPost("/api/payment/webhook")]
    public async Task<IActionResult> PaymentWebhook(PaymentWebhookRequest request, CancellationToken cancellationToken)
    {
        var payment = await _dbContext.Payments
            .Include(currentPayment => currentPayment.Booking)
                .ThenInclude(booking => booking!.BookingPassengers)
                    .ThenInclude(passenger => passenger.SeatAvailability)
                        .ThenInclude(seatAvailability => seatAvailability!.Seat)
            .FirstOrDefaultAsync(currentPayment => currentPayment.GatewayOrderId == request.OrderId, cancellationToken);

        if (payment is null || payment.Booking is null)
        {
            return NotFound(new { message = "Payment was not found." });
        }

        var normalizedStatus = request.Status.Trim().ToUpperInvariant();

        if (normalizedStatus == "SUCCESS")
        {
            var now = DateTime.UtcNow;
            var invalidSeat = payment.Booking.BookingPassengers.FirstOrDefault(passenger =>
                passenger.SeatAvailability is null ||
                passenger.SeatAvailability.Status != "Held" ||
                !passenger.SeatAvailability.HoldExpiry.HasValue ||
                passenger.SeatAvailability.HoldExpiry.Value <= now);

            if (invalidSeat is not null)
            {
                payment.Status = "FAILED";
                payment.FailureReason = "Seat hold expired before booking confirmation.";
                payment.Booking.Status = "FAILED";

                foreach (var passenger in payment.Booking.BookingPassengers)
                {
                    if (passenger.SeatAvailability is not null && passenger.SeatAvailability.Status == "Held")
                    {
                        passenger.SeatAvailability.Status = "Available";
                        passenger.SeatAvailability.HoldExpiry = null;
                    }
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                return BadRequest(new { message = "Seat is not valid for booking." });
            }

            payment.Status = "SUCCESS";
            payment.GatewayPaymentId = request.PaymentId?.Trim();
            payment.PaymentReference = request.PaymentReference?.Trim() ?? payment.PaymentReference;
            payment.FailureReason = null;
            payment.Booking.Status = "CONFIRMED";

            foreach (var passenger in payment.Booking.BookingPassengers)
            {
                if (passenger.SeatAvailability is not null)
                {
                    passenger.SeatAvailability.Status = "Booked";
                    passenger.SeatAvailability.HoldExpiry = null;
                }
            }
        }
        else
        {
            payment.Status = "FAILED";
            payment.GatewayPaymentId = request.PaymentId?.Trim() ?? payment.GatewayPaymentId;
            payment.PaymentReference = request.PaymentReference?.Trim() ?? payment.PaymentReference;
            payment.FailureReason = request.FailureReason?.Trim() ?? payment.FailureReason;
            payment.Booking.Status = "FAILED";

            foreach (var passenger in payment.Booking.BookingPassengers)
            {
                if (passenger.SeatAvailability is not null)
                {
                    passenger.SeatAvailability.Status = "Available";
                    passenger.SeatAvailability.HoldExpiry = null;
                }
            }
        }

        if (normalizedStatus == "SUCCESS")
        {
            payment.PaidAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok();
    }

    [HttpPost("{paymentId:int}/confirm")]
    public async Task<ActionResult<BookingResponse>> ConfirmPayment(int paymentId, ConfirmPaymentRequest request, CancellationToken cancellationToken)
    {
        var payment = await GetUserPaymentAsync(paymentId, cancellationToken);
        if (payment is null)
        {
            return NotFound(new { message = "Payment was not found." });
        }

        if (payment.Status == "SUCCESS")
        {
            return BadRequest(new { message = "Payment is already confirmed." });
        }

        if (payment.Status == "FAILED")
        {
            return BadRequest(new { message = "Failed payment cannot be confirmed." });
        }

        if (payment.Booking is null)
        {
            return BadRequest(new { message = "Payment is not linked to a booking." });
        }

        if (_paymentService.GetExpiryUtc(payment.Booking.CreatedAt) < DateTime.UtcNow)
        {
            payment.Status = "FAILED";
            payment.FailureReason = "Payment window expired.";
            payment.Booking.Status = "FAILED";

            foreach (var passenger in payment.Booking.BookingPassengers)
            {
                if (passenger.SeatAvailability is not null)
                {
                    passenger.SeatAvailability.Status = "Available";
                    passenger.SeatAvailability.HoldExpiry = null;
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return BadRequest(new { message = "Payment window has expired." });
        }

        payment.Status = "SUCCESS";
        payment.GatewayPaymentId = request.GatewayPaymentId.Trim();
        payment.PaymentReference = request.PaymentReference?.Trim() ?? payment.PaymentReference;
        payment.PaidAt = DateTime.UtcNow;
        payment.Booking.Status = "CONFIRMED";

        foreach (var passenger in payment.Booking.BookingPassengers)
        {
            if (passenger.SeatAvailability is not null)
            {
                passenger.SeatAvailability.Status = "Booked";
                passenger.SeatAvailability.HoldExpiry = null;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var emailSent = false;
        try
        {
            emailSent = await _bookingEmailService.TrySendTicketAsync(payment.Booking, cancellationToken);
        }
        catch
        {
            emailSent = false;
        }

        return Ok(ToBookingResponse(payment.Booking, emailSent));
    }

    [HttpPost("{paymentId:int}/fail")]
    public async Task<ActionResult<PaymentResponse>> FailPayment(int paymentId, FailPaymentRequest request, CancellationToken cancellationToken)
    {
        var payment = await GetUserPaymentAsync(paymentId, cancellationToken);
        if (payment is null)
        {
            return NotFound(new { message = "Payment was not found." });
        }

        if (payment.Status == "SUCCESS")
        {
            return BadRequest(new { message = "Successful payment cannot be failed." });
        }

        payment.Status = "FAILED";
        payment.FailureReason = request.FailureReason.Trim();
        payment.PaymentReference = request.PaymentReference?.Trim() ?? payment.PaymentReference;

        if (payment.Booking is not null && payment.Booking.Status == "PENDING")
        {
            payment.Booking.Status = "FAILED";

            foreach (var passenger in payment.Booking.BookingPassengers)
            {
                if (passenger.SeatAvailability is not null)
                {
                    passenger.SeatAvailability.Status = "Available";
                    passenger.SeatAvailability.HoldExpiry = null;
                }
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(ToPaymentResponse(payment));
    }

    [HttpGet("{paymentId:int}")]
    public async Task<ActionResult<PaymentResponse>> GetPayment(int paymentId, CancellationToken cancellationToken)
    {
        var payment = await GetUserPaymentAsync(paymentId, cancellationToken, asNoTracking: true);
        if (payment is null)
        {
            return NotFound(new { message = "Payment was not found." });
        }

        return Ok(ToPaymentResponse(payment));
    }

    [HttpGet]
    public async Task<ActionResult<List<PaymentResponse>>> GetMyPayments(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var payments = await _dbContext.Payments
            .AsNoTracking()
            .Include(payment => payment.Booking)
            .Where(payment => payment.Booking != null && payment.Booking.UserId == userId.Value)
            .OrderByDescending(payment => payment.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(payments.Select(ToPaymentResponse).ToList());
    }

    private int? GetUserId()
    {
        return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
            ? userId
            : null;
    }

    private async Task<Payment?> GetUserPaymentAsync(int paymentId, CancellationToken cancellationToken, bool asNoTracking = false)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return null;
        }

        var query = _dbContext.Payments
            .Include(payment => payment.Booking)
                .ThenInclude(booking => booking!.User)
            .Include(payment => payment.Booking)
                .ThenInclude(booking => booking!.Schedule)
                    .ThenInclude(schedule => schedule!.Bus)
                        .ThenInclude(bus => bus!.Operator)
            .Include(payment => payment.Booking)
                .ThenInclude(booking => booking!.Schedule)
                    .ThenInclude(schedule => schedule!.Route)
                        .ThenInclude(route => route!.SourceCity)
            .Include(payment => payment.Booking)
                .ThenInclude(booking => booking!.Schedule)
                    .ThenInclude(schedule => schedule!.Route)
                        .ThenInclude(route => route!.DestinationCity)
            .Include(payment => payment.Booking)
                .ThenInclude(booking => booking!.BookingPassengers)
                    .ThenInclude(passenger => passenger.SeatAvailability)
                        .ThenInclude(seatAvailability => seatAvailability!.Seat)
            .Include(payment => payment.Booking)
                .ThenInclude(booking => booking!.Payments)
            .Where(payment => payment.Id == paymentId && payment.Booking != null && payment.Booking.UserId == userId.Value);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    private PaymentResponse ToPaymentResponse(Payment payment)
    {
        var booking = payment.Booking!;
        return new PaymentResponse(
            payment.Id,
            booking.Id,
            payment.Amount ?? 0,
            payment.PaymentMethod ?? string.Empty,
            payment.GatewayOrderId,
            payment.GatewayPaymentId,
            payment.PaymentReference,
            payment.Status ?? "PENDING",
            payment.RefundStatus ?? "NONE",
            payment.FailureReason,
            payment.CreatedAt,
            payment.PaidAt,
            _paymentService.GetExpiryUtc(booking.CreatedAt));
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
