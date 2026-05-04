using BusBookingApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusBookingApp.Controllers;

[ApiController]
[Authorize(Roles = "ADMIN")]
[Route("api/admin/revenue")]
public class AdminRevenueController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public AdminRevenueController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetRevenue(CancellationToken cancellationToken)
    {
        var bookingsQuery = _dbContext.Bookings
            .AsNoTracking()
            .AsQueryable();

        var adminRevenue = await bookingsQuery
            .Where(booking => booking.Status == "CONFIRMED")
            .SumAsync(booking => booking.GstAmount + booking.ConvenienceFee, cancellationToken);
        var totalBookings = await bookingsQuery.CountAsync(cancellationToken);
        var totalCancelled = await bookingsQuery
            .Where(booking => booking.Status == "CANCELLED")
            .CountAsync(cancellationToken);

        return Ok(new
        {
            adminRevenue,
            totalBookings,
            totalCancelled
        });
    }
}
