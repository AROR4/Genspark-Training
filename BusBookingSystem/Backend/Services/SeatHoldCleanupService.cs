using BusBookingApp.Data;
using Microsoft.EntityFrameworkCore;

namespace BusBookingApp.Services;

public class SeatHoldCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SeatHoldCleanupService> _logger;

    public SeatHoldCleanupService(IServiceScopeFactory scopeFactory, ILogger<SeatHoldCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await ReleaseExpiredSeatHoldsAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown.
        }
    }

    private async Task ReleaseExpiredSeatHoldsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var now = DateTime.UtcNow;

        var expiredSeats = await dbContext.SeatAvailabilities
            .Include(seatAvailability => seatAvailability.Seat)
            .Where(seatAvailability =>
                seatAvailability.Status == "Held" &&
                seatAvailability.HoldExpiry.HasValue &&
                seatAvailability.HoldExpiry.Value <= now)
            .ToListAsync(cancellationToken);

        if (expiredSeats.Count == 0)
        {
            return;
        }

        var affectedBookingIds = await dbContext.BookingPassengers
            .Where(passenger => passenger.SeatAvailabilityId.HasValue && expiredSeats.Select(seat => seat.Id).Contains(passenger.SeatAvailabilityId.Value))
            .Select(passenger => passenger.BookingId)
            .Where(bookingId => bookingId.HasValue)
            .Distinct()
            .Select(bookingId => bookingId!.Value)
            .ToListAsync(cancellationToken);

        foreach (var seatAvailability in expiredSeats)
        {
            seatAvailability.Status = "Available";
            seatAvailability.HoldExpiry = null;
        }

        var bookings = await dbContext.Bookings
            .Include(booking => booking.BookingPassengers)
                .ThenInclude(passenger => passenger.SeatAvailability)
            .Where(booking => affectedBookingIds.Contains(booking.Id) && booking.Status == "PENDING")
            .ToListAsync(cancellationToken);

        foreach (var booking in bookings)
        {
            var anyHeldSeatRemaining = booking.BookingPassengers.Any(passenger =>
                passenger.SeatAvailability is not null &&
                passenger.SeatAvailability.Status == "Held" &&
                passenger.SeatAvailability.HoldExpiry.HasValue &&
                passenger.SeatAvailability.HoldExpiry.Value > now);

            if (!anyHeldSeatRemaining)
            {
                booking.Status = "FAILED";
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Released {Count} expired seat holds.", expiredSeats.Count);
    }
}
