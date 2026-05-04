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
[Route("api/operator")]
public class OperatorController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly CancellationService _cancellationService;

    public OperatorController(AppDbContext context, CancellationService cancellationService)
    {
        _context = context;
        _cancellationService = cancellationService;
    }

    [Authorize(Roles = "OPERATOR")]
    [HttpGet("trips")]
    public async Task<ActionResult<List<OperatorTripResponse>>> GetMyTrips()
    {
        var currentUserOperatorId = await GetCurrentOperatorIdAsync();
        if (currentUserOperatorId is null)
        {
            return Forbid();
        }

        var now = DateTime.UtcNow;

        var schedules = await _context.BusSchedules
            .AsNoTracking()
            .Include(schedule => schedule.Bus)
            .Include(schedule => schedule.Route)
                .ThenInclude(route => route!.SourceCity)
            .Include(schedule => schedule.Route)
                .ThenInclude(route => route!.DestinationCity)
            .Include(schedule => schedule.SeatAvailabilities)
            .Include(schedule => schedule.Bookings)
                .ThenInclude(booking => booking.User)
            .Include(schedule => schedule.Bookings)
                .ThenInclude(booking => booking.BookingPassengers)
                    .ThenInclude(passenger => passenger.SeatAvailability)
                        .ThenInclude(seatAvailability => seatAvailability!.Seat)
            .Where(schedule => schedule.Bus != null && schedule.Bus.OperatorId == currentUserOperatorId.Value)
            .OrderByDescending(schedule => schedule.TravelDate)
            .ThenBy(schedule => schedule.DepartureTime)
            .ToListAsync();

        var response = schedules.Select(schedule =>
        {
            var bookedSeats = schedule.SeatAvailabilities.Count(seatAvailability => seatAvailability.Status == "Booked");
            var onHoldSeats = schedule.SeatAvailabilities.Count(seatAvailability =>
                seatAvailability.Status == "Held" &&
                seatAvailability.HoldExpiry.HasValue &&
                seatAvailability.HoldExpiry.Value > now);
            var totalSeats = schedule.Bus?.TotalSeats ?? 0;
            var availableSeats = Math.Max(0, totalSeats - bookedSeats - onHoldSeats);

            var currentBookings = schedule.Bookings
                .Where(booking => booking.Status != "FAILED" && booking.Status != "EXPIRED" && booking.Status != "CANCELLED")
                .OrderByDescending(booking => booking.CreatedAt)
                .Select(booking => new OperatorTripBookingResponse(
                    booking.Id,
                    booking.Status,
                    booking.TotalAmount,
                    booking.User?.Name,
                    booking.User?.Email,
                    booking.User?.PhoneNumber,
                    booking.ContactEmail,
                    booking.ContactPhone,
                    booking.CreatedAt,
                    booking.BookingPassengers.Count,
                    booking.BookingPassengers
                        .OrderBy(passenger => passenger.SeatAvailability!.Seat!.SeatNumber)
                        .Select(passenger => new OperatorTripPassengerResponse(
                            passenger.Name ?? string.Empty,
                            passenger.Age ?? 0,
                            passenger.Gender ?? string.Empty,
                            passenger.SeatAvailability?.Seat?.SeatNumber ?? string.Empty))
                        .ToList()))
                .ToList();

            return new OperatorTripResponse(
                schedule.Id,
                schedule.BusId ?? schedule.Bus?.Id ?? 0,
                schedule.Bus?.RegistrationNumber ?? string.Empty,
                schedule.Bus?.Company ?? string.Empty,
                schedule.Bus?.Type ?? 0,
                schedule.Route?.SourceCity?.Name,
                schedule.Route?.DestinationCity?.Name,
                schedule.TravelDate,
                schedule.DepartureTime,
                schedule.ArrivalDate,
                schedule.ArrivalTime,
                schedule.DurationMinutes,
                schedule.BasePrice,
                schedule.IsCancelled,
                totalSeats,
                bookedSeats,
                onHoldSeats,
                availableSeats,
                currentBookings.Count,
                currentBookings);
        }).ToList();

        return Ok(response);
    }

    [Authorize(Roles = "OPERATOR")]
    [HttpPost("bus/{busId:int}/disable")]
    public async Task<IActionResult> DisableBus(int busId)
    {
        var currentUserOperatorId = await GetCurrentOperatorIdAsync();
        if (currentUserOperatorId is null)
        {
            return Forbid();
        }

        var bus = await _context.Buses
            .Include(b => b.BusSchedules)
            .FirstOrDefaultAsync(b => b.Id == busId);

        if (bus is null)
        {
            return NotFound();
        }

        if (bus.OperatorId != currentUserOperatorId)
        {
            return Forbid();
        }

        bus.IsActive = false;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        foreach (var schedule in bus.BusSchedules.Where(currentSchedule => currentSchedule.TravelDate >= today))
        {
            await _cancellationService.CancelSchedule(schedule.Id, CancellationService.OperatorBusCancellation);
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Bus disabled and future schedules cancelled.",
            busId = bus.Id,
            isActive = bus.IsActive
        });
    }

    [Authorize(Roles = "OPERATOR")]
    [HttpPost("trips/{scheduleId:int}/cancel")]
    public async Task<IActionResult> CancelTrip(int scheduleId, [FromBody] CancelOperatorTripRequest? request)
    {
        var currentUserOperatorId = await GetCurrentOperatorIdAsync();
        if (currentUserOperatorId is null)
        {
            return Forbid();
        }

        var schedule = await _context.BusSchedules
            .Include(currentSchedule => currentSchedule.Bus)
            .FirstOrDefaultAsync(currentSchedule => currentSchedule.Id == scheduleId);

        if (schedule is null)
        {
            return NotFound(new { message = "Trip was not found." });
        }

        if (schedule.Bus?.OperatorId != currentUserOperatorId.Value)
        {
            return Forbid();
        }

        if (schedule.IsCancelled)
        {
            return BadRequest(new { message = "Trip is already cancelled." });
        }

        await _cancellationService.CancelSchedule(schedule.Id, CancellationService.OperatorTripCancellation);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Trip cancelled successfully.",
            scheduleId = schedule.Id,
            isCancelled = true
        });
    }

    private async Task<int?> GetCurrentOperatorIdAsync()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdValue, out var userId))
        {
            return null;
        }

        var busOperator = await _context.BusOperators
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.UserId == userId && o.IsActive);

        return busOperator?.Id;
    }
}
