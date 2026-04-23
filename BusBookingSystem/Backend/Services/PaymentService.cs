namespace BusBookingApp.Services;

public class PaymentService
{
    private readonly IConfiguration _configuration;

    public PaymentService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public int GetSeatHoldMinutes()
    {
        return _configuration.GetValue("Payments:SeatHoldMinutes", 15);
    }

    public DateTime GetExpiryUtc(DateTime createdAtUtc)
    {
        return createdAtUtc.AddMinutes(GetSeatHoldMinutes());
    }

    public DateTime GetActiveSeatHoldCutoffUtc()
    {
        return DateTime.UtcNow.AddMinutes(-GetSeatHoldMinutes());
    }

    public string GenerateGatewayOrderId()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..36];
    }
}
