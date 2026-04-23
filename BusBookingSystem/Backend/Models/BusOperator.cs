namespace BusBookingApp.Models;

public class BusOperator
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string? CompanyName { get; set; }
    public string? LegalName { get; set; }
    public string? OwnerName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? TaxNumber { get; set; }
    public string? LicenseNumber { get; set; }
    public string ApprovalStatus { get; set; } = "PENDING";
    public string? AdminNotes { get; set; }
    public DateTime CreatedAt { get; set; }

    public User? User { get; set; }
    public ICollection<OperatorOffice> OperatorOffices { get; set; } = new List<OperatorOffice>();
    public ICollection<Bus> Buses { get; set; } = new List<Bus>();
}
