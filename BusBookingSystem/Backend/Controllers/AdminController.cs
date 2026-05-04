using BusBookingApp.Data;
using BusBookingApp.Dtos;
using BusBookingApp.Models;
using BusBookingApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusBookingApp.Controllers;

[ApiController]
[Authorize(Roles = "ADMIN")]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly CancellationService _cancellationService;

    public AdminController(AppDbContext dbContext, CancellationService cancellationService)
    {
        _dbContext = dbContext;
        _cancellationService = cancellationService;
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPost("route")]
    public async Task<IActionResult> AddRoute(CreateRouteDto dto)
    {
        var route = new BusRoute
        {
            SourceCityId = dto.SourceCityId,
            DestinationCityId = dto.DestinationCityId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _dbContext.Routes.Add(route);
        await _dbContext.SaveChangesAsync();

        return Ok(route);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpDelete("route/{id:int}")]
    public async Task<IActionResult> RemoveRoute(int id)
    {
        var route = await _dbContext.Routes.FindAsync(id);

        if (route is null)
        {
            return NotFound();
        }

        route.IsActive = false;
        await _dbContext.SaveChangesAsync();

        return Ok(new { message = "Route disabled." });
    }

    [Authorize(Roles = "ADMIN")]
    [HttpGet("route")]
    public async Task<IActionResult> GetRoutes([FromQuery] bool includeInactive = false)
    {
        var query = _dbContext.Routes
            .AsNoTracking()
            .Include(route => route.SourceCity)
            .Include(route => route.DestinationCity)
            .AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(route => route.IsActive);
        }

        var routes = await query
            .OrderByDescending(route => route.CreatedAt)
            .Select(route => new
            {
                route.Id,
                route.SourceCityId,
                SourceCityName = route.SourceCity != null ? route.SourceCity.Name : null,
                route.DestinationCityId,
                DestinationCityName = route.DestinationCity != null ? route.DestinationCity.Name : null,
                route.IsActive,
                route.CreatedAt
            })
            .ToListAsync();

        return Ok(routes);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpGet("route/{id:int}")]
    public async Task<IActionResult> GetRouteById(int id)
    {
        var route = await _dbContext.Routes
            .AsNoTracking()
            .Include(currentRoute => currentRoute.SourceCity)
            .Include(currentRoute => currentRoute.DestinationCity)
            .Where(currentRoute => currentRoute.Id == id)
            .Select(currentRoute => new
            {
                currentRoute.Id,
                currentRoute.SourceCityId,
                SourceCityName = currentRoute.SourceCity != null ? currentRoute.SourceCity.Name : null,
                currentRoute.DestinationCityId,
                DestinationCityName = currentRoute.DestinationCity != null ? currentRoute.DestinationCity.Name : null,
                currentRoute.IsActive,
                currentRoute.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (route is null)
        {
            return NotFound(new { message = "Route not found." });
        }

        return Ok(route);
    }

    [HttpGet("operator-requests")]
    public async Task<ActionResult<List<OperatorRequestResponse>>> GetOperatorRequests([FromQuery] string status = "PENDING")
    {
        var normalizedStatus = status.Trim().ToUpperInvariant();

        var operators = await _dbContext.BusOperators
            .AsNoTracking()
            .Include(busOperator => busOperator.User)
            .Include(busOperator => busOperator.OperatorOffices)
                .ThenInclude(office => office.City)
            .Where(busOperator => busOperator.ApprovalStatus == normalizedStatus)
            .OrderBy(busOperator => busOperator.CreatedAt)
            .ToListAsync();

        return Ok(operators.Select(ToResponse).ToList());
    }

    [Authorize(Roles = "ADMIN")]
    [HttpGet("operators")]
    public async Task<IActionResult> GetAllOperators([FromQuery] string? userSearch = null)
    {
        var query = _dbContext.BusOperators
            .AsNoTracking()
            .Include(busOperator => busOperator.User)
            .Include(busOperator => busOperator.OperatorOffices)
                .ThenInclude(office => office.City)
            .Include(busOperator => busOperator.Buses)
                .ThenInclude(bus => bus.BusSchedules)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(userSearch))
        {
            var normalized = userSearch.Trim().ToLower();
            query = query.Where(busOperator =>
                busOperator.User != null &&
                ((busOperator.User.Name != null && busOperator.User.Name.ToLower().Contains(normalized)) ||
                 (busOperator.User.Email != null && busOperator.User.Email.ToLower().Contains(normalized)) ||
                 (busOperator.User.PhoneNumber != null && busOperator.User.PhoneNumber.ToLower().Contains(normalized))));
        }

        var operators = await query
            .OrderByDescending(busOperator => busOperator.CreatedAt)
            .Select(busOperator => new
            {
                busOperator.Id,
                busOperator.UserId,
                busOperator.OwnerName,
                busOperator.CompanyName,
                busOperator.LegalName,
                busOperator.ContactEmail,
                busOperator.ContactPhone,
                busOperator.RegistrationNumber,
                busOperator.TaxNumber,
                busOperator.LicenseNumber,
                busOperator.ApprovalStatus,
                busOperator.IsActive,
                busOperator.AdminNotes,
                busOperator.CreatedAt,
                User = busOperator.User == null ? null : new
                {
                    busOperator.User.Id,
                    busOperator.User.Name,
                    busOperator.User.Email,
                    busOperator.User.PhoneNumber,
                    busOperator.User.Role,
                    busOperator.User.IsApproved,
                    busOperator.User.CreatedAt
                },
                Offices = busOperator.OperatorOffices
                    .Select(office => new
                    {
                        office.Id,
                        office.CityId,
                        CityName = office.City != null ? office.City.Name : null,
                        office.Address
                    })
                    .ToList(),
                Buses = busOperator.Buses
                    .OrderByDescending(bus => bus.CreatedAt)
                    .Select(bus => new
                    {
                        bus.Id,
                        bus.TotalSeats,
                        bus.LayoutJson,
                        bus.IsActive,
                        bus.CreatedAt,
                        Schedules = bus.BusSchedules
                            .OrderByDescending(schedule => schedule.TravelDate)
                            .ThenBy(schedule => schedule.DepartureTime)
                            .Select(schedule => new
                            {
                                schedule.Id,
                                schedule.RouteId,
                                schedule.TravelDate,
                                schedule.DepartureTime,
                                schedule.ArrivalDate,
                                schedule.ArrivalTime,
                                schedule.DurationMinutes,
                                schedule.BasePrice,
                                schedule.IsCancelled,
                                schedule.CreatedAt
                            })
                            .ToList()
                    })
                    .ToList()
            })
            .ToListAsync();

        return Ok(operators);
    }

    [HttpGet("operator-requests/{operatorId:int}")]
    public async Task<ActionResult<OperatorRequestResponse>> GetOperatorRequest(int operatorId)
    {
        var busOperator = await _dbContext.BusOperators
            .AsNoTracking()
            .Include(busOperator => busOperator.User)
            .Include(busOperator => busOperator.OperatorOffices)
                .ThenInclude(office => office.City)
            .FirstOrDefaultAsync(busOperator => busOperator.Id == operatorId);

        if (busOperator is null)
        {
            return NotFound(new { message = "Operator request was not found." });
        }

        return Ok(ToResponse(busOperator));
    }

    [HttpPost("operator-requests/{operatorId:int}/approve")]
    public async Task<IActionResult> ApproveOperator(int operatorId, OperatorDecisionRequest request)
    {
        var busOperator = await _dbContext.BusOperators
            .Include(busOperator => busOperator.User)
            .FirstOrDefaultAsync(busOperator => busOperator.Id == operatorId);

        if (busOperator is null)
        {
            return NotFound(new { message = "Operator request was not found." });
        }

        busOperator.ApprovalStatus = "APPROVED";
        busOperator.AdminNotes = request.AdminNotes?.Trim();

        if (busOperator.User is not null)
        {
            busOperator.User.IsApproved = true;
        }

        await _dbContext.SaveChangesAsync();

        return Ok(new { message = "Operator approved successfully." });
    }

    [HttpPost("operator-requests/{operatorId:int}/reject")]
    public async Task<IActionResult> RejectOperator(int operatorId, OperatorDecisionRequest request)
    {
        var busOperator = await _dbContext.BusOperators
            .Include(busOperator => busOperator.User)
            .FirstOrDefaultAsync(busOperator => busOperator.Id == operatorId);

        if (busOperator is null)
        {
            return NotFound(new { message = "Operator request was not found." });
        }

        busOperator.ApprovalStatus = "REJECTED";
        busOperator.AdminNotes = request.AdminNotes?.Trim();

        if (busOperator.User is not null)
        {
            busOperator.User.IsApproved = false;
        }

        await _dbContext.SaveChangesAsync();

        return Ok(new { message = "Operator rejected." });
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPost("operator/{id:int}/disable")]
    public async Task<IActionResult> DisableOperator(int id)
    {
        var operatorEntity = await _dbContext.BusOperators
            .Include(o => o.User)
            .Include(o => o.Buses)
                .ThenInclude(b => b.BusSchedules)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (operatorEntity is null)
        {
            return NotFound();
        }

        operatorEntity.IsActive = false;
        operatorEntity.ApprovalStatus = "DISABLED";
        if (operatorEntity.User is not null)
        {
            operatorEntity.User.IsApproved = false;
        }
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        foreach (var bus in operatorEntity.Buses)
        {
            bus.IsActive = false;

            foreach (var schedule in bus.BusSchedules.Where(currentSchedule => currentSchedule.TravelDate >= today))
            {
                await _cancellationService.CancelSchedule(schedule.Id, CancellationService.AdminCancellation);
            }
        }

        await _dbContext.SaveChangesAsync();

        return Ok(new
        {
            message = "Operator disabled with all buses and schedules.",
            operatorId = operatorEntity.Id,
            approvalStatus = operatorEntity.ApprovalStatus,
            isActive = operatorEntity.IsActive,
            userIsApproved = operatorEntity.User?.IsApproved
        });
    }

    private static OperatorRequestResponse ToResponse(Models.BusOperator busOperator)
    {
        return new OperatorRequestResponse(
            busOperator.Id,
            busOperator.UserId ?? 0,
            busOperator.OwnerName,
            busOperator.User?.Email,
            busOperator.User?.PhoneNumber,
            busOperator.CompanyName,
            busOperator.LegalName,
            busOperator.ContactEmail,
            busOperator.ContactPhone,
            busOperator.RegistrationNumber,
            busOperator.TaxNumber,
            busOperator.LicenseNumber,
            busOperator.ApprovalStatus,
            busOperator.AdminNotes,
            busOperator.CreatedAt,
            busOperator.OperatorOffices
                .Select(office => new OperatorOfficeResponse(office.Id, office.City?.Name, office.Address))
                .ToList());
    }
}
