namespace BusBookingApp.Models;

public class BusRoute
{
    public int Id { get; set; }
    public int? SourceCityId { get; set; }
    public int? DestinationCityId { get; set; }
    public DateTime CreatedAt { get; set; }

    public City? SourceCity { get; set; }
    public City? DestinationCity { get; set; }
    public ICollection<BusSchedule> BusSchedules { get; set; } = new List<BusSchedule>();
}
