namespace BusBookingApp.Models;

public class OperatorOffice
{
    public int Id { get; set; }
    public int? OperatorId { get; set; }
    public int? CityId { get; set; }
    public string? Address { get; set; }

    public BusOperator? Operator { get; set; }
    public City? City { get; set; }
}
