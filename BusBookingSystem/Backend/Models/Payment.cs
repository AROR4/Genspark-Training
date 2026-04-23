namespace BusBookingApp.Models;

public class Payment
{
    public int Id { get; set; }
    public int? BookingId { get; set; }
    public decimal? Amount { get; set; }
    public string? PaymentMethod { get; set; }
    public string? PaymentReference { get; set; }
    public string? GatewayOrderId { get; set; }
    public string? GatewayPaymentId { get; set; }
    public string? Status { get; set; }
    public string? FailureReason { get; set; }
    public string? RefundStatus { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? RefundedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public Booking? Booking { get; set; }
}
