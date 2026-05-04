namespace BusBookingApp.Models;

public class Booking
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ScheduleId { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal GstAmount { get; set; }
    public decimal ConvenienceFee { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "PENDING";
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public decimal? RefundAmount { get; set; }
    public decimal? OperatorLoss { get; set; }
    public decimal? AdminRevenue { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationType { get; set; }
    public DateTime CreatedAt { get; set; }

    public User? User { get; set; }
    public BusSchedule? Schedule { get; set; }
    public ICollection<BookingPassenger> BookingPassengers { get; set; } = new List<BookingPassenger>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
