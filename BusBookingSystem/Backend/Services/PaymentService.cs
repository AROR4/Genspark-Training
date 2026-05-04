using BusBookingApp.Data;
using Microsoft.EntityFrameworkCore;

namespace BusBookingApp.Services;

public class PaymentService
{
    private readonly AppDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private const decimal GstRate = 0.05m;
    private const decimal ConvenienceFeeFixed = 50m;

    public PaymentService(AppDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public int GetSeatHoldMinutes()
    {
        return _configuration.GetValue("Payments:SeatHoldMinutes", 10);
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

    public (decimal BaseAmount, decimal GstAmount, decimal ConvenienceFee, decimal TotalAmount) CalculateBookingAmount(
        decimal seatBasePrice,
        int seatCount)
    {
        var baseAmount = seatBasePrice * seatCount;
        var gstAmount = baseAmount * GstRate;
        var convenienceFee = ConvenienceFeeFixed;
        var totalAmount = baseAmount + gstAmount + convenienceFee;

        return (baseAmount, gstAmount, convenienceFee, totalAmount);
    }

    public async Task RefundAsync(int bookingId, decimal amount)
    {
        var payment = await _dbContext.Payments
            .Where(currentPayment => currentPayment.BookingId == bookingId)
            .OrderByDescending(currentPayment => currentPayment.CreatedAt)
            .FirstOrDefaultAsync();

        if (payment is null)
        {
            return;
        }

        payment.RefundStatus = "COMPLETED";
        payment.RefundedAt = DateTime.UtcNow;

        // TODO: integrate real gateway later.
    }
}
