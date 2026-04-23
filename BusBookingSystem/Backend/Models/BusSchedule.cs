namespace BusBookingApp.Models;

public class BusSchedule
{
    public int Id { get; set; }
    public int? BusId { get; set; }
    public int? RouteId { get; set; }
    public DateOnly TravelDate { get; set; }
    public TimeOnly DepartureTime { get; set; }
    public int DurationMinutes { get; set; }
    public DateOnly ArrivalDate { get; set; }
    public TimeOnly ArrivalTime { get; set; }
    public decimal BasePrice { get; set; }
    public DateTime CreatedAt { get; set; }

    public Bus? Bus { get; set; }
    public BusRoute? Route { get; set; }
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
