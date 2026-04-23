using System.Security.Claims;
using BusBookingApp.Data;
using BusBookingApp.Dtos;
using BusBookingApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusBookingApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OperatorsController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;

    public OperatorsController(AppDbContext dbContext, IPasswordHasher<User> passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
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
            var cityName = officeRequest.CityName.Trim();
            var cityNameLower = cityName.ToLowerInvariant();
            var city = await _dbContext.Cities.FirstOrDefaultAsync(city => city.Name.ToLower() == cityNameLower);

            city ??= new City { Name = cityName };

            busOperator.OperatorOffices.Add(new OperatorOffice
            {
                City = city,
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
    public async Task<ActionResult<BusResponse>> AddBus(CreateBusRequest request)
    {
        var busOperator = await GetApprovedOperatorAsync();
        if (busOperator is null)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Only approved operators can add buses." });
        }

        var bus = new Bus
        {
            OperatorId = busOperator.Id,
            TotalSeats = request.TotalSeats,
            LayoutJson = request.LayoutJson,
            IsActive = request.IsActive
        };

        for (var seatNumber = 1; seatNumber <= request.TotalSeats; seatNumber++)
        {
            bus.Seats.Add(new Seat { SeatNumber = seatNumber.ToString() });
        }

        _dbContext.Buses.Add(bus);
        await _dbContext.SaveChangesAsync();

        return Created($"/api/operators/buses/{bus.Id}", ToBusResponse(bus, busOperator.Id));
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
    [HttpPost("schedules")]
    public async Task<ActionResult<BusScheduleResponse>> AddBusSchedule(CreateBusScheduleRequest request)
    {
        var busOperator = await GetApprovedOperatorAsync();
        if (busOperator is null)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Only approved operators can add bus schedules." });
        }

        var bus = await _dbContext.Buses
            .FirstOrDefaultAsync(bus => bus.Id == request.BusId && bus.OperatorId == busOperator.Id);

        if (bus is null)
        {
            return NotFound(new { message = "Bus was not found for this operator." });
        }

        if (!bus.IsActive)
        {
            return BadRequest(new { message = "Cannot create a schedule for an inactive bus." });
        }

        var sourceCity = await GetOrCreateCityAsync(request.SourceCityName);
        var destinationCity = await GetOrCreateCityAsync(request.DestinationCityName);

        if (string.Equals(sourceCity.Name, destinationCity.Name, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Source city and destination city must be different." });
        }

        var route = await _dbContext.Routes.FirstOrDefaultAsync(route =>
            route.SourceCityId == sourceCity.Id && route.DestinationCityId == destinationCity.Id);

        route ??= new BusRoute
        {
            SourceCity = sourceCity,
            DestinationCity = destinationCity
        };

        var departureDateTime = request.TravelDate.ToDateTime(request.DepartureTime);
        var arrivalDateTime = departureDateTime.AddMinutes(request.DurationMinutes);

        var schedule = new BusSchedule
        {
            Bus = bus,
            Route = route,
            TravelDate = request.TravelDate,
            DepartureTime = request.DepartureTime,
            DurationMinutes = request.DurationMinutes,
            ArrivalDate = DateOnly.FromDateTime(arrivalDateTime),
            ArrivalTime = TimeOnly.FromDateTime(arrivalDateTime),
            BasePrice = request.BasePrice
        };

        _dbContext.BusSchedules.Add(schedule);
        await _dbContext.SaveChangesAsync();

        return Created($"/api/operators/schedules/{schedule.Id}", ToScheduleResponse(schedule, route, sourceCity, destinationCity));
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
                busOperator.User != null &&
                busOperator.User.IsApproved);
    }

    private async Task<City> GetOrCreateCityAsync(string cityName)
    {
        var normalizedName = cityName.Trim();
        var normalizedNameLower = normalizedName.ToLowerInvariant();

        var city = await _dbContext.Cities.FirstOrDefaultAsync(city => city.Name.ToLower() == normalizedNameLower);
        return city ?? new City { Name = normalizedName };
    }

    private static BusResponse ToBusResponse(Bus bus, int operatorId)
    {
        return new BusResponse(
            bus.Id,
            operatorId,
            bus.TotalSeats,
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
