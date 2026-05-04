using BusBookingApp.Data;
using BusBookingApp.Dtos;
using BusBookingApp.Models;
using BusBookingApp.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusBookingApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly JwtTokenService _jwtTokenService;

    public AuthController(
        AppDbContext dbContext,
        IPasswordHasher<User> passwordHasher,
        JwtTokenService jwtTokenService)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher; 
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("signup")]
    public async Task<ActionResult<AuthResponse>> SignUp(SignUpRequest request)
    {
        var email = NormalizeEmail(request.Email);

        if (await _dbContext.Users.AnyAsync(user => user.Email == email))
        {
            return Conflict(new { message = "Email is already registered." });
        }

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = email,
            PhoneNumber = request.PhoneNumber?.Trim(),
            Role = "USER",
            IsApproved = true
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var (token, expiresAt) = _jwtTokenService.CreateToken(user);

        return Ok(new AuthResponse(
            user.Id,
            user.Name,
            user.Email,
            user.PhoneNumber,
            user.Role,
            user.IsApproved,
            token,
            expiresAt));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var email = NormalizeEmail(request.Email);
        var user = await _dbContext.Users.FirstOrDefaultAsync(user => user.Email == email);

        if (user is null)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var passwordResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (passwordResult == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        if (passwordResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            await _dbContext.SaveChangesAsync();
        }

        if (string.Equals(user.Role, "OPERATOR", StringComparison.OrdinalIgnoreCase) && !user.IsApproved)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Operator account is waiting for admin approval." });
        }

        var (token, expiresAt) = _jwtTokenService.CreateToken(user);

        return Ok(new AuthResponse(
            user.Id,
            user.Name,
            user.Email,
            user.PhoneNumber,
            user.Role,
            user.IsApproved,
            token,
            expiresAt));
    }

    [Authorize]
    [HttpGet("validate-token")]
    public async Task<ActionResult<ValidateTokenResponse>> ValidateToken()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid token payload." });
        }

        var user = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(user => user.Id == userId);
        if (user is null)
        {
            return Unauthorized(new { message = "User no longer exists." });
        }

        if (string.Equals(user.Role, "OPERATOR", StringComparison.OrdinalIgnoreCase) && !user.IsApproved)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Operator account is waiting for admin approval." });
        }

        return Ok(new ValidateTokenResponse(
            true,
            user.Id,
            user.Name,
            user.Email,
            user.Role,
            user.IsApproved));
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
