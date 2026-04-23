using System.ComponentModel.DataAnnotations;

namespace BusBookingApp.Dtos;

public record OperatorOfficeRequest(
    [Required, MaxLength(100)] string CityName,
    [Required] string Address);

public record OperatorRegisterRequest(
    [Required, MaxLength(100)] string OwnerName,
    [Required, EmailAddress, MaxLength(150)] string Email,
    [Phone, MaxLength(20)] string? PhoneNumber,
    [Required, MinLength(8), RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).+$",
        ErrorMessage = "Password must contain uppercase, lowercase, number, and special character.")]
    string Password,
    [Required, MaxLength(150)] string CompanyName,
    [MaxLength(150)] string? LegalName,
    [Required, EmailAddress, MaxLength(150)] string ContactEmail,
    [Required, Phone, MaxLength(20)] string ContactPhone,
    [Required, MaxLength(100)] string RegistrationNumber,
    [MaxLength(100)] string? TaxNumber,
    [Required, MaxLength(100)] string LicenseNumber,
    [Required, MinLength(1)] List<OperatorOfficeRequest> Offices);

public record OperatorOfficeResponse(
    int Id,
    string? CityName,
    string? Address);

public record OperatorRegistrationResponse(
    int OperatorId,
    int UserId,
    string CompanyName,
    string ApprovalStatus,
    string Message);

public record OperatorRequestResponse(
    int OperatorId,
    int UserId,
    string? OwnerName,
    string? Email,
    string? PhoneNumber,
    string? CompanyName,
    string? LegalName,
    string? ContactEmail,
    string? ContactPhone,
    string? RegistrationNumber,
    string? TaxNumber,
    string? LicenseNumber,
    string ApprovalStatus,
    string? AdminNotes,
    DateTime CreatedAt,
    List<OperatorOfficeResponse> Offices);

public record OperatorDecisionRequest(
    [MaxLength(1000)] string? AdminNotes);

public record CreateBusRequest(
    [Range(1, 100)] int TotalSeats,
    string? LayoutJson,
    bool IsActive = true);

public record BusResponse(
    int BusId,
    int OperatorId,
    int TotalSeats,
    string? LayoutJson,
    bool IsActive,
    DateTime CreatedAt,
    List<string> Seats);

public record CreateBusScheduleRequest(
    [Required] int BusId,
    [Required, MaxLength(100)] string SourceCityName,
    [Required, MaxLength(100)] string DestinationCityName,
    [Required] DateOnly TravelDate,
    [Required] TimeOnly DepartureTime,
    [Range(1, 10080)] int DurationMinutes,
    [Range(1, 1000000)] decimal BasePrice);

public record BusScheduleResponse(
    int ScheduleId,
    int BusId,
    int RouteId,
    string? SourceCityName,
    string? DestinationCityName,
    DateOnly TravelDate,
    TimeOnly DepartureTime,
    int DurationMinutes,
    DateOnly ArrivalDate,
    TimeOnly ArrivalTime,
    decimal BasePrice,
    DateTime CreatedAt);
