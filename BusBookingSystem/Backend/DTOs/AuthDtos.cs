using System.ComponentModel.DataAnnotations;

namespace BusBookingApp.Dtos;

public record SignUpRequest(
    [Required, MaxLength(100)] string Name,
    [Required, EmailAddress, MaxLength(150)] string Email,
    [Phone, MaxLength(20)] string? PhoneNumber,
    [Required, MinLength(8), RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).+$",
        ErrorMessage = "Password must contain uppercase, lowercase, number, and special character.")]
    string Password);

public record LoginRequest(
    [Required, EmailAddress, MaxLength(150)] string Email,
    [Required] string Password);

public record AuthResponse(
    int UserId,
    string? Name,
    string Email,
    string? PhoneNumber,
    string? Role,
    bool IsApproved,
    string Token,
    DateTime ExpiresAt);
