using BusBookingApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusBookingApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CitiesController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public CitiesController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetCities()
    {
        var cities = await _dbContext.Cities
            .AsNoTracking()
            .OrderBy(city => city.Name)
            .Select(city => new
            {
                city.Id,
                city.Name
            })
            .ToListAsync();

        return Ok(cities);
    }
}
