namespace BusBookingApp.Models;

public class City
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<OperatorOffice> OperatorOffices { get; set; } = new List<OperatorOffice>();
    public ICollection<BusRoute> SourceRoutes { get; set; } = new List<BusRoute>();
    public ICollection<BusRoute> DestinationRoutes { get; set; } = new List<BusRoute>();
}
