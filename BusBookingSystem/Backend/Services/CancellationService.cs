using BusBookingApp.Data;
using BusBookingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BusBookingApp.Services;

public class CancellationService
{
    public const string AdminCancellation = "ADMIN";
    public const string OperatorBusCancellation = "OPERATOR_BUS";
    public const string OperatorTripCancellation = "OPERATOR_TRIP";
    public const string UserCancellation = "USER";

    private readonly AppDbContext _dbContext;
    private readonly PaymentService _paymentService;
    private readonly BookingEmailService _bookingEmailService;

    public CancellationService(
        AppDbContext dbContext,
        PaymentService paymentService,
        BookingEmailService bookingEmailService)
    {
        _dbContext = dbContext;
        _paymentService = paymentService;
        _bookingEmailService = bookingEmailService;
    }

    public decimal CalculateUserRefundPercentage(DateTime departureDateTime, DateTime cancelledAt)
    {
        var hoursUntilDeparture = (departureDateTime - cancelledAt).TotalHours;

        if (hoursUntilDeparture >= 24)
        {
            return 100m;
        }

        if (hoursUntilDeparture >= 12)
        {
            return 50m;
        }

        if (hoursUntilDeparture >= 4)
        {
            return 25m;
        }

        return 0m;
    }

    public async Task<(Booking Booking, decimal RefundPercentage, decimal RefundAmount, bool EmailSent, bool CancelledNow)?> CancelUserBookingAsync(
        int bookingId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var booking = await _dbContext.Bookings
            .Include(currentBooking => currentBooking.User)
            .Include(currentBooking => currentBooking.Schedule)
                .ThenInclude(schedule => schedule!.Bus)
                    .ThenInclude(bus => bus!.Operator)
            .Include(currentBooking => currentBooking.Schedule)
                .ThenInclude(schedule => schedule!.Route)
                    .ThenInclude(route => route!.SourceCity)
            .Include(currentBooking => currentBooking.Schedule)
                .ThenInclude(schedule => schedule!.Route)
                    .ThenInclude(route => route!.DestinationCity)
            .Include(currentBooking => currentBooking.BookingPassengers)
                .ThenInclude(passenger => passenger.SeatAvailability)
                    .ThenInclude(seatAvailability => seatAvailability!.Seat)
            .Include(currentBooking => currentBooking.Payments)
            .FirstOrDefaultAsync(currentBooking => currentBooking.Id == bookingId && currentBooking.UserId == userId, cancellationToken);

        if (booking is null)
        {
            return null;
        }

        if (booking.Status == "CANCELLED" || booking.Status == "FAILED" || booking.Status == "EXPIRED")
        {
            return (booking, 0m, booking.RefundAmount ?? 0m, false, false);
        }

        var originalStatus = booking.Status;
        var now = DateTime.Now;
        var departureDateTime = booking.Schedule!.TravelDate.ToDateTime(booking.Schedule.DepartureTime);
        var refundPercentage = originalStatus == "CONFIRMED"
            ? CalculateUserRefundPercentage(departureDateTime, now)
            : 0m;
        var refundAmount = originalStatus == "CONFIRMED"
            ? decimal.Round(booking.TotalAmount * refundPercentage / 100m, 2, MidpointRounding.AwayFromZero)
            : 0m;

        booking.Status = "CANCELLED";
        booking.RefundAmount = refundAmount;
        booking.OperatorLoss = 0m;
        booking.AdminRevenue = 0m;
        booking.CancellationType = UserCancellation;
        booking.CancelledAt = DateTime.UtcNow;

        foreach (var passenger in booking.BookingPassengers)
        {
            if (passenger.SeatAvailability is not null)
            {
                passenger.SeatAvailability.Status = "Available";
                passenger.SeatAvailability.HoldExpiry = null;
            }
        }

        if (originalStatus == "CONFIRMED" && refundAmount > 0)
        {
            await _paymentService.RefundAsync(booking.Id, refundAmount);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var emailSent = false;
        try
        {
            emailSent = await _bookingEmailService.TrySendCancellationAsync(booking, cancellationToken);
        }
        catch
        {
            emailSent = false;
        }

        return (booking, refundPercentage, refundAmount, emailSent, true);
    }

    public async Task CancelSchedule(int scheduleId, string cancelledBy)
    {
        var schedule = await _dbContext.BusSchedules
            .Include(currentSchedule => currentSchedule.Bookings)
                .ThenInclude(booking => booking.BookingPassengers)
                    .ThenInclude(passenger => passenger.SeatAvailability)
            .Include(currentSchedule => currentSchedule.Bookings)
                .ThenInclude(booking => booking.Payments)
            .Include(currentSchedule => currentSchedule.Bookings)
                .ThenInclude(booking => booking.User)
            .Include(currentSchedule => currentSchedule.Route)
                .ThenInclude(route => route!.SourceCity)
            .Include(currentSchedule => currentSchedule.Route)
                .ThenInclude(route => route!.DestinationCity)
            .Include(currentSchedule => currentSchedule.Bus)
                .ThenInclude(bus => bus!.Operator)
            .FirstOrDefaultAsync(currentSchedule => currentSchedule.Id == scheduleId);

        if (schedule is null || schedule.IsCancelled)
        {
            return;
        }

        schedule.IsCancelled = true;

        foreach (var booking in schedule.Bookings)
        {
            if (booking.Status == "PENDING")
            {
                booking.Status = "CANCELLED";
                booking.CancelledAt = DateTime.UtcNow;
                booking.CancellationType = cancelledBy;

                foreach (var passenger in booking.BookingPassengers)
                {
                    if (passenger.SeatAvailability is not null)
                    {
                        passenger.SeatAvailability.Status = "Available";
                        passenger.SeatAvailability.HoldExpiry = null;
                    }
                }

                continue;
            }

            if (booking.Status != "CONFIRMED")
            {
                continue;
            }

            var refundAmount = booking.TotalAmount;
            decimal operatorLoss;
            decimal adminRevenue;

            if (cancelledBy == AdminCancellation)
            {
                operatorLoss = booking.BaseAmount * 0.5m;
                adminRevenue = 0m;
            }
            else if (cancelledBy == OperatorBusCancellation || cancelledBy == OperatorTripCancellation)
            {
                operatorLoss = booking.BaseAmount;
                adminRevenue = booking.GstAmount + booking.ConvenienceFee;
            }
            else
            {
                operatorLoss = 0m;
                adminRevenue = 0m;
            }

            booking.Status = "CANCELLED";
            booking.RefundAmount = refundAmount;
            booking.OperatorLoss = operatorLoss;
            booking.AdminRevenue = adminRevenue;
            booking.CancellationType = cancelledBy;
            booking.CancelledAt = DateTime.UtcNow;

            await _paymentService.RefundAsync(booking.Id, refundAmount);

            try
            {
                await _bookingEmailService.TrySendCancellationAsync(booking);
            }
            catch
            {
                // Cancellation and refund updates should still succeed even if email delivery fails.
            }
        }
    }
}
