using System.ComponentModel.DataAnnotations;

namespace BusBookingApp.Dtos;

public record ScheduleSearchResponse(
    int ScheduleId,
    int BusId,
    string? OperatorName,
    string? SourceCityName,
    string? DestinationCityName,
    DateOnly TravelDate,
    TimeOnly DepartureTime,
    DateOnly ArrivalDate,
    TimeOnly ArrivalTime,
    int DurationMinutes,
    decimal BasePrice,
    int TotalSeats,
    int AvailableSeats);

public record SearchBusResponse(
    int ScheduleId,
    int BusId,
    string? OperatorName,
    string? SourceCity,
    string? DestinationCity,
    TimeOnly DepartureTime,
    TimeOnly ArrivalTime,
    int Duration,
    decimal Price,
    int AvailableSeats);

public record BookingJourneyResponse(
    int ScheduleId,
    int BusId,
    string RegistrationNumber,
    string? OperatorName,
    string? SourceCityName,
    string? DestinationCityName,
    DateOnly TravelDate,
    TimeOnly DepartureTime,
    DateOnly ArrivalDate,
    TimeOnly ArrivalTime,
    int DurationMinutes,
    decimal BasePrice,
    int TotalSeats);

public record SeatAvailabilityResponse(
    int SeatAvailabilityId,
    string SeatNumber,
    string Status,
    DateTime? HoldExpiry);

public record BookingPassengerRequest(
    [Required, MaxLength(100)] string Name,
    [Range(0, 120)] int Age,
    [Required, MaxLength(10)] string Gender,
    [Required] int SeatAvailabilityId);

public record CreateBookingRequest(
    [Required] int ScheduleId,
    [Required, MinLength(1)] List<BookingPassengerRequest> Passengers,
    [EmailAddress, MaxLength(150)] string? ContactEmail,
    [Phone, MaxLength(20)] string? ContactPhone,
    [Required, MaxLength(50)] string PaymentMethod,
    [MaxLength(100)] string? PaymentReference);

public record TicketPassengerResponse(
    string Name,
    int Age,
    string Gender,
    string SeatNumber);

public record BookingResponse(
    int BookingId,
    string BookingCode,
    string TicketNumber,
    string Status,
    decimal BaseAmount,
    decimal GstAmount,
    decimal ConvenienceFee,
    decimal TotalAmount,
    string PaymentStatus,
    DateTime BookedAt,
    string? ContactEmail,
    string? ContactPhone,
    decimal? RefundAmount,
    decimal? OperatorLoss,
    decimal? AdminRevenue,
    string? CancellationType,
    BookingJourneyResponse Journey,
    List<TicketPassengerResponse> Passengers,
    bool EmailSent);

public record CancelBookingResponse(
    string Message,
    decimal RefundPercentage,
    decimal RefundAmount,
    BookingResponse Booking);

public record TicketEmailResponse(
    int BookingId,
    string Email,
    bool Sent,
    string Message);

public record InitPaymentRequest(
    [Required] int BookingId);

public record PaymentWebhookRequest(
    [Required, MaxLength(100)] string OrderId,
    [Required, MaxLength(20)] string Status,
    [MaxLength(100)] string? PaymentId,
    [MaxLength(100)] string? PaymentReference,
    [MaxLength(500)] string? FailureReason);
