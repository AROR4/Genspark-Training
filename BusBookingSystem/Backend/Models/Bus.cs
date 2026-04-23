namespace BusBookingApp.Models;

public class Bus
{
    public int Id { get; set; }
    public int? OperatorId { get; set; }
    public int TotalSeats { get; set; }
    public string? LayoutJson { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public BusOperator? Operator { get; set; }
    public ICollection<BusSchedule> BusSchedules { get; set; } = new List<BusSchedule>();
    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
}
