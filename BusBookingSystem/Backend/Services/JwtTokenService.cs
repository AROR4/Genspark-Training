using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BusBookingApp.Models;
using Microsoft.IdentityModel.Tokens;

namespace BusBookingApp.Services;

public class JwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string Token, DateTime ExpiresAt) CreateToken(User user)
    {
        var issuer = _configuration["Jwt:Issuer"] ?? "BusBookingSystem";
        var audience = _configuration["Jwt:Audience"] ?? "BusBookingSystem";
        var key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured.");
        var expiresAt = DateTime.UtcNow.AddMinutes(_configuration.GetValue("Jwt:ExpiresMinutes", 60));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role ?? "USER")
        };

        if (!string.IsNullOrWhiteSpace(user.Name))
        {
            claims.Add(new Claim(ClaimTypes.Name, user.Name));
        }

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
