using BusBookingApp.Data;
using BusBookingApp.Dtos;
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

    public AdminController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
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
