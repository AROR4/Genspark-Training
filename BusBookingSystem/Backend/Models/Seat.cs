namespace BusBookingApp.Models;

public class Seat
{
    public int Id { get; set; }
    public int? BusId { get; set; }
    public string? SeatNumber { get; set; }

    public Bus? Bus { get; set; }
    public ICollection<SeatAvailability> SeatAvailabilities { get; set; } = new List<SeatAvailability>();
}
