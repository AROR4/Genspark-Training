using System.Security.Claims;
using BusBookingApp.Data;
using BusBookingApp.Dtos;
using BusBookingApp.Models;
using BusBookingApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusBookingApp.Controllers;

[ApiController]
[Route("api/operator")]
public class OperatorsController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly SeatLayoutService _seatLayoutService;

    public OperatorsController(AppDbContext dbContext, IPasswordHasher<User> passwordHasher, SeatLayoutService seatLayoutService)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _seatLayoutService = seatLayoutService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<OperatorRegistrationResponse>> Register(OperatorRegisterRequest request)
    {
        var email = NormalizeEmail(request.Email);

        if (await _dbContext.Users.AnyAsync(user => user.Email == email))
        {
            return Conflict(new { message = "Email is already registered." });
        }

        if (request.Offices.Count == 0)
        {
            return BadRequest(new { message = "At least one operator office is required." });
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        var user = new User
        {
            Name = request.OwnerName.Trim(),
            Email = email,
            PhoneNumber = request.PhoneNumber?.Trim(),
            Role = "OPERATOR",
            IsApproved = false
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        var busOperator = new BusOperator
        {
            User = user,
            CompanyName = request.CompanyName.Trim(),
            LegalName = request.LegalName?.Trim(),
            OwnerName = request.OwnerName.Trim(),
            ContactEmail = NormalizeEmail(request.ContactEmail),
            ContactPhone = request.ContactPhone.Trim(),
            RegistrationNumber = request.RegistrationNumber.Trim(),
            TaxNumber = request.TaxNumber?.Trim(),
            LicenseNumber = request.LicenseNumber.Trim(),
            ApprovalStatus = "PENDING"
        };

        foreach (var officeRequest in request.Offices)
        {
            var cityExists = await _dbContext.Cities
                .AnyAsync(city => city.Id == officeRequest.CityId);

            if (!cityExists)
            {
                return BadRequest(new { message = $"City with id {officeRequest.CityId} was not found." });
            }

            busOperator.OperatorOffices.Add(new OperatorOffice
            {
                CityId = officeRequest.CityId,
                Address = officeRequest.Address.Trim()
            });
        }

        _dbContext.BusOperators.Add(busOperator);
        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        return Created($"/api/admin/operator-requests/{busOperator.Id}", new OperatorRegistrationResponse(
            busOperator.Id,
            user.Id,
            busOperator.CompanyName,
            busOperator.ApprovalStatus,
            "Operator registration submitted. Admin approval is required before login."));
    }

    [Authorize(Roles = "OPERATOR")]
    [HttpPost("buses")]
    public async Task<ActionResult<BusResponse>> AddBus([FromBody] CreateBusRequest request)
    {
        var busOperator = await GetApprovedOperatorAsync();
        if (busOperator is null)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Only approved operators can add buses." });
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        var bus = new Bus
        {
            OperatorId = busOperator.Id,
            TotalSeats = request.TotalSeats,
            RegistrationNumber = request.RegistrationNumber.Trim(),
            Company = request.Company.Trim(),
            Type = request.Type,
            LayoutJson = _seatLayoutService.GenerateLayoutJson(request.TotalSeats),
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Buses.Add(bus);
        await _dbContext.SaveChangesAsync();

        var seats = _seatLayoutService.GenerateSeats(bus.Id, request.TotalSeats);
        bus.Seats = seats;
        _dbContext.Seats.AddRange(seats);
        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        return Created($"/api/operator/buses/{bus.Id}", ToBusResponse(bus, busOperator.Id));
    }

    [Authorize(Roles = "OPERATOR")]
    [HttpGet("buses")]
    public async Task<ActionResult<List<BusResponse>>> GetMyBuses()
    {
        var busOperator = await GetApprovedOperatorAsync();
        if (busOperator is null)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Only approved operators can view buses." });
        }

        var buses = await _dbContext.Buses
            .AsNoTracking()
            .Include(bus => bus.Seats)
            .Where(bus => bus.OperatorId == busOperator.Id)
            .OrderByDescending(bus => bus.CreatedAt)
            .ToListAsync();

        return Ok(buses.Select(bus => ToBusResponse(bus, busOperator.Id)).ToList());
    }

    [Authorize(Roles = "OPERATOR")]
    [HttpGet("routes")]
    public async Task<ActionResult<List<OperatorRouteAvailabilityResponse>>> GetOperatorRoutes()
    {
        var busOperator = await GetApprovedOperatorAsync();
        if (busOperator is null)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Only approved operators can view routes." });
        }

        var officeCityIdList = await _dbContext.OperatorOffices
            .AsNoTracking()
            .Where(office => office.OperatorId == busOperator.Id && office.CityId.HasValue)
            .Select(office => office.CityId!.Value)
            .Distinct()
            .ToListAsync();
        var officeCityIds = officeCityIdList.ToHashSet();

        var routes = await _dbContext.Routes
            .AsNoTracking()
            .Include(route => route.SourceCity)
            .Include(route => route.DestinationCity)
            .Where(route => route.IsActive)
            .OrderBy(route => route.Id)
            .ToListAsync();

        var response = routes
            .Where(route => route.SourceCityId.HasValue && route.DestinationCityId.HasValue)
            .Select(route =>
            {
                var sourceCityId = route.SourceCityId!.Value;
                var destinationCityId = route.DestinationCityId!.Value;
                var hasSourceOffice = officeCityIds.Contains(sourceCityId);
                var hasDestinationOffice = officeCityIds.Contains(destinationCityId);

                var missingCities = new List<string>();
                if (!hasSourceOffice)
                {
                    missingCities.Add(route.SourceCity?.Name ?? "Unknown");
                }

                if (!hasDestinationOffice)
                {
                    missingCities.Add(route.DestinationCity?.Name ?? "Unknown");
                }

                return new OperatorRouteAvailabilityResponse(
                    route.Id,
                    sourceCityId,
                    route.SourceCity?.Name ?? string.Empty,
                    destinationCityId,
                    route.DestinationCity?.Name ?? string.Empty,
                    hasSourceOffice,
                    hasDestinationOffice,
                    hasSourceOffice && hasDestinationOffice,
                    missingCities);
            })
            .ToList();

        return Ok(response);
    }

    [Authorize(Roles = "OPERATOR")]
    [HttpPost("office")]
    public async Task<ActionResult<OperatorOfficeResponse>> AddOffice(AddOperatorOfficeRequest request)
    {
        var busOperator = await GetApprovedOperatorAsync();
        if (busOperator is null)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Only approved operators can add offices." });
        }

        var city = await _dbContext.Cities.AsNoTracking().FirstOrDefaultAsync(city => city.Id == request.CityId);
        if (city is null)
        {
            return BadRequest(new { message = "Invalid city id." });
        }

        var hasDuplicate = await _dbContext.OperatorOffices
            .AnyAsync(office => office.OperatorId == busOperator.Id && office.CityId == request.CityId);

        if (hasDuplicate)
        {
            return Conflict(new { message = "Office already exists for this city." });
        }

        var office = new OperatorOffice
        {
            OperatorId = busOperator.Id,
            CityId = request.CityId,
            Address = request.Address.Trim()
        };

        _dbContext.OperatorOffices.Add(office);
        await _dbContext.SaveChangesAsync();

        return Created($"/api/operator/office/{office.Id}",
            new OperatorOfficeResponse(office.Id, city.Name, office.Address));
    }

    [Authorize(Roles = "OPERATOR")]
    [HttpGet("offices")]
    public async Task<ActionResult<List<OperatorOfficeResponse>>> GetMyOffices()
    {
        var busOperator = await GetApprovedOperatorAsync();
        if (busOperator is null)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Only approved operators can view offices." });
        }

        var offices = await _dbContext.OperatorOffices
            .AsNoTracking()
            .Include(office => office.City)
            .Where(office => office.OperatorId == busOperator.Id)
            .OrderBy(office => office.City!.Name)
            .ThenBy(office => office.Id)
            .Select(office => new OperatorOfficeResponse(
                office.Id,
                office.City != null ? office.City.Name : null,
                office.Address))
            .ToListAsync();

        return Ok(offices);
    }

    [Authorize(Roles = "OPERATOR")]
    [HttpPost("schedule")]
    public async Task<ActionResult<BusScheduleResponse>> CreateSchedule(CreateOperatorScheduleRequest request)
    {
        var busOperator = await GetApprovedOperatorAsync();
        if (busOperator is null)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Only approved operators can create schedules." });
        }

        if (!busOperator.IsActive)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Disabled operators cannot add schedules." });
        }

        var bus = await _dbContext.Buses
            .Include(currentBus => currentBus.Seats)
            .FirstOrDefaultAsync(currentBus => currentBus.Id == request.BusId && currentBus.OperatorId == busOperator.Id);

        if (bus is null)
        {
            return NotFound(new { message = "Bus was not found for this operator." });
        }

        if (!bus.IsActive)
        {
            return BadRequest(new { message = "Cannot create a schedule for an inactive bus." });
        }

        var route = await _dbContext.Routes
            .Include(currentRoute => currentRoute.SourceCity)
            .Include(currentRoute => currentRoute.DestinationCity)
            .FirstOrDefaultAsync(currentRoute => currentRoute.Id == request.RouteId && currentRoute.IsActive);

        if (route is null || !route.SourceCityId.HasValue || !route.DestinationCityId.HasValue)
        {
            return BadRequest(new { message = "Route does not exist." });
        }

        var sourceOffice = await _dbContext.OperatorOffices
            .FirstOrDefaultAsync(office => office.Id == request.SourceOfficeId && office.OperatorId == busOperator.Id);
        if (sourceOffice is null)
        {
            return BadRequest(new { message = "Source office does not exist." });
        }

        var destinationOffice = await _dbContext.OperatorOffices
            .FirstOrDefaultAsync(office => office.Id == request.DestinationOfficeId && office.OperatorId == busOperator.Id);
        if (destinationOffice is null)
        {
            return BadRequest(new { message = "Destination office does not exist." });
        }

        if (sourceOffice.CityId != route.SourceCityId || destinationOffice.CityId != route.DestinationCityId)
        {
            return BadRequest(new { message = "Office does not match route" });
        }

        var departureDateTime = request.TravelDate.ToDateTime(request.DepartureTime);
        var arrivalDateTime = departureDateTime.AddMinutes(request.DurationMinutes);
        var bufferMinutes = 30;

        var existingSchedules = await _dbContext.BusSchedules
            .AsNoTracking()
            .Where(schedule => schedule.BusId == request.BusId && !schedule.IsCancelled)
            .ToListAsync();

        foreach (var existingSchedule in existingSchedules)
        {
            var existingDepartureDateTime = existingSchedule.TravelDate.ToDateTime(existingSchedule.DepartureTime);
            var existingArrivalDateTime = existingDepartureDateTime
                .AddMinutes(existingSchedule.DurationMinutes)
                .AddMinutes(bufferMinutes);

            if (departureDateTime < existingArrivalDateTime && arrivalDateTime > existingDepartureDateTime)
            {
                return BadRequest(new
                {
                    message = $"Bus already booked from {existingDepartureDateTime} to {existingArrivalDateTime}"
                });
            }
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        var schedule = new BusSchedule
        {
            BusId = bus.Id,
            RouteId = route.Id,
            SourceOfficeId = sourceOffice.Id,
            DestinationOfficeId = destinationOffice.Id,
            TravelDate = request.TravelDate,
            DepartureTime = request.DepartureTime,
            DurationMinutes = request.DurationMinutes,
            ArrivalDate = DateOnly.FromDateTime(arrivalDateTime),
            ArrivalTime = TimeOnly.FromDateTime(arrivalDateTime),
            BasePrice = request.BasePrice,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.BusSchedules.Add(schedule);
        await _dbContext.SaveChangesAsync();

        foreach (var seat in bus.Seats)
        {
            _dbContext.SeatAvailabilities.Add(new SeatAvailability
            {
                BusScheduleId = schedule.Id,
                SeatId = seat.Id,
                Status = "Available"
            });
        }

        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        return Created($"/api/operator/schedule/{schedule.Id}", ToScheduleResponse(
            schedule,
            route,
            route.SourceCity!,
            route.DestinationCity!));
    }

    [Authorize(Roles = "OPERATOR")]
    [HttpGet("schedules")]
    public async Task<ActionResult<List<BusScheduleResponse>>> GetMySchedules()
    {
        var busOperator = await GetApprovedOperatorAsync();
        if (busOperator is null)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Only approved operators can view schedules." });
        }

        var schedules = await _dbContext.BusSchedules
            .AsNoTracking()
            .Include(schedule => schedule.Bus)
            .Include(schedule => schedule.Route)
                .ThenInclude(route => route!.SourceCity)
            .Include(schedule => schedule.Route)
                .ThenInclude(route => route!.DestinationCity)
            .Where(schedule => schedule.Bus != null && schedule.Bus.OperatorId == busOperator.Id)
            .OrderByDescending(schedule => schedule.TravelDate)
            .ThenBy(schedule => schedule.DepartureTime)
            .ToListAsync();

        return Ok(schedules.Select(schedule => ToScheduleResponse(
            schedule,
            schedule.Route!,
            schedule.Route!.SourceCity!,
            schedule.Route!.DestinationCity!)).ToList());
    }

    private async Task<BusOperator?> GetApprovedOperatorAsync()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdValue, out var userId))
        {
            return null;
        }

        return await _dbContext.BusOperators
            .Include(busOperator => busOperator.User)
            .FirstOrDefaultAsync(busOperator =>
                busOperator.UserId == userId &&
                busOperator.ApprovalStatus == "APPROVED" &&
                busOperator.IsActive &&
                busOperator.User != null &&
                busOperator.User.IsApproved);
    }

    private static BusResponse ToBusResponse(Bus bus, int operatorId)
    {
        return new BusResponse(
            bus.Id,
            operatorId,
            bus.TotalSeats,
            bus.RegistrationNumber,
            bus.Company,
            bus.Type,
            bus.LayoutJson,
            bus.IsActive,
            bus.CreatedAt,
            bus.Seats.Select(seat => seat.SeatNumber ?? string.Empty).OrderBy(seat => seat).ToList());
    }

    private static BusScheduleResponse ToScheduleResponse(
        BusSchedule schedule,
        BusRoute route,
        City sourceCity,
        City destinationCity)
    {
        return new BusScheduleResponse(
            schedule.Id,
            schedule.BusId ?? schedule.Bus?.Id ?? 0,
            schedule.RouteId ?? route.Id,
            schedule.SourceOfficeId,
            schedule.DestinationOfficeId,
            sourceCity.Name,
            destinationCity.Name,
            schedule.TravelDate,
            schedule.DepartureTime,
            schedule.DurationMinutes,
            schedule.ArrivalDate,
            schedule.ArrivalTime,
            schedule.BasePrice,
            schedule.CreatedAt);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
