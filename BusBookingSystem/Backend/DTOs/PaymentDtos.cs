using System.ComponentModel.DataAnnotations;

namespace BusBookingApp.Dtos;

public record PaymentCheckoutResponse(
    int BookingId,
    int PaymentId,
    string BookingStatus,
    string PaymentStatus,
    string GatewayOrderId,
    decimal Amount,
    string PaymentMethod,
    DateTime ExpiresAtUtc);

public record ConfirmPaymentRequest(
    [Required, MaxLength(100)] string GatewayPaymentId,
    [MaxLength(100)] string? PaymentReference);

public record FailPaymentRequest(
    [Required, MaxLength(500)] string FailureReason,
    [MaxLength(100)] string? PaymentReference);

public record PaymentResponse(
    int PaymentId,
    int BookingId,
    decimal Amount,
    string PaymentMethod,
    string? GatewayOrderId,
    string? GatewayPaymentId,
    string? PaymentReference,
    string Status,
    string RefundStatus,
    string? FailureReason,
    DateTime CreatedAt,
    DateTime? PaidAt,
    DateTime ExpiresAtUtc);
