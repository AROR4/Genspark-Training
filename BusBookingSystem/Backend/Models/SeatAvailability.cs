namespace BusBookingApp.Models;

public class SeatAvailability
{
    public int Id { get; set; }

    public int BusScheduleId { get; set; }
    public int SeatId { get; set; }

    public string Status { get; set; } = "Available";
    public DateTime? HoldExpiry { get; set; }

    public BusSchedule BusSchedule { get; set; } = null!;
    public Seat Seat { get; set; } = null!;
}