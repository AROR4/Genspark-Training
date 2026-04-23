namespace BusBookingApp.Models;

public class User
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string? Role { get; set; }
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<BusOperator> BusOperators { get; set; } = new List<BusOperator>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
