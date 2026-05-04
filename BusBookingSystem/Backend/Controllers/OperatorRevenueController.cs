using System.Security.Claims;
using BusBookingApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusBookingApp.Controllers;

[ApiController]
[Authorize(Roles = "OPERATOR")]
[Route("api/operator/revenue")]
public class OperatorRevenueController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public OperatorRevenueController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetRevenue(CancellationToken cancellationToken)
    {
        var operatorId = await GetCurrentOperatorIdAsync(cancellationToken);
        if (operatorId is null)
        {
            return Forbid();
        }

        var operatorBookingsQuery = _dbContext.Bookings
            .AsNoTracking()
            .Where(booking =>
                booking.Schedule != null &&
                booking.Schedule.Bus != null &&
                booking.Schedule.Bus.OperatorId == operatorId.Value);

        var totalRevenue = await operatorBookingsQuery
            .Where(booking => booking.Status == "CONFIRMED")
            .SumAsync(booking => booking.BaseAmount, cancellationToken);

        var totalBookings = await operatorBookingsQuery.CountAsync(cancellationToken);
        var totalCancelled = await operatorBookingsQuery
            .Where(booking => booking.Status == "CANCELLED")
            .CountAsync(cancellationToken);

        var totalTickets = await _dbContext.BookingPassengers
            .AsNoTracking()
            .Where(passenger =>
                passenger.Booking != null &&
                passenger.Booking.Status == "CONFIRMED" &&
                passenger.Booking.Schedule != null &&
                passenger.Booking.Schedule.Bus != null &&
                passenger.Booking.Schedule.Bus.OperatorId == operatorId.Value)
            .CountAsync(cancellationToken);

        return Ok(new
        {
            totalRevenue,
            totalTickets,
            totalBookings,
            totalCancelled
        });
    }

    private async Task<int?> GetCurrentOperatorIdAsync(CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdValue, out var userId))
        {
            return null;
        }

        var busOperator = await _dbContext.BusOperators
            .AsNoTracking()
            .FirstOrDefaultAsync(currentOperator => currentOperator.UserId == userId && currentOperator.IsActive, cancellationToken);

        return busOperator?.Id;
    }
}
