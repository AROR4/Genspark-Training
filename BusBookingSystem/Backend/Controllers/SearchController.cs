using BusBookingApp.Data;
using BusBookingApp.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusBookingApp.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    private readonly AppDbContext _context;

    public SearchController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("buses")]
    public async Task<ActionResult<List<SearchBusResponse>>> SearchBuses(
        [FromQuery] int sourceCityId,
        [FromQuery] int destinationCityId,
        [FromQuery] DateOnly date)
    {
        var now = DateTime.UtcNow;

        if (sourceCityId <= 0 || destinationCityId <= 0)
        {
            return BadRequest(new { message = "Source and destination city IDs must be valid." });
        }

        if (sourceCityId == destinationCityId)
        {
            return BadRequest(new { message = "Source and destination city IDs must be different." });
        }

        var schedules = await _context.BusSchedules
            .AsNoTracking()
            .Include(schedule => schedule.Bus)
                .ThenInclude(bus => bus!.Operator)
            .Include(schedule => schedule.Route)
            .Include(schedule => schedule.SourceOffice)
                .ThenInclude(office => office!.City)
            .Include(schedule => schedule.DestinationOffice)
                .ThenInclude(office => office!.City)
            .Where(schedule =>
                schedule.Route != null &&
                schedule.Bus != null &&
                schedule.SourceOffice != null &&
                schedule.DestinationOffice != null &&
                schedule.Route.SourceCityId == sourceCityId &&
                schedule.Route.DestinationCityId == destinationCityId &&
                schedule.TravelDate == date &&
                !schedule.IsCancelled &&
                schedule.Bus.IsActive &&
                schedule.Route.IsActive)
            .OrderBy(schedule => schedule.DepartureTime)
            .ToListAsync();

        var scheduleIds = schedules.Select(schedule => schedule.Id).ToList();
        var availableSeatsBySchedule = await _context.SeatAvailabilities
            .AsNoTracking()
            .Where(seatAvailability =>
                scheduleIds.Contains(seatAvailability.BusScheduleId) &&
                (seatAvailability.Status == "Available" ||
                 (seatAvailability.Status == "Held" &&
                  seatAvailability.HoldExpiry.HasValue &&
                  seatAvailability.HoldExpiry.Value <= now)))
            .GroupBy(seatAvailability => seatAvailability.BusScheduleId)
            .Select(group => new { ScheduleId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(group => group.ScheduleId, group => group.Count);

        var result = schedules.Select(schedule => new SearchBusResponse(
            schedule.Id,
            schedule.BusId ?? 0,
            schedule.Bus!.Operator?.CompanyName,
            schedule.SourceOffice!.City?.Name,
            schedule.DestinationOffice!.City?.Name,
            schedule.DepartureTime,
            schedule.ArrivalTime,
            schedule.DurationMinutes,
            schedule.BasePrice,
            availableSeatsBySchedule.GetValueOrDefault(schedule.Id)
        )).ToList();

        return Ok(result);
    }
}
