namespace BusBookingApp.Models;

public class BookingPassenger
{
    public int Id { get; set; }
    public int? BookingId { get; set; }
    public string? Name { get; set; }
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public int? SeatId { get; set; }

    public Booking? Booking { get; set; }
    public Seat? Seat { get; set; }
}
