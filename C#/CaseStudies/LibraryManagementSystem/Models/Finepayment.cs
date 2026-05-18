using System;

namespace LibraryManagementSystem.Models;

public class FinePayment
{
    public int FinePaymentId { get; set; }

    public int FineId { get; set; }

    public decimal AmountPaid { get; set; }

    public DateTime PaymentDate { get; set; }

    public string? PaymentMethod { get; set; }

    public Fine Fine { get; set; } = null!;

    public override string ToString()
    {
        return $"Id: {FinePaymentId} | Fine Id: {FineId} | " +
            $"Amount Paid: {AmountPaid:C} | " +
            $"Payment Date: {PaymentDate:g} | " +
            $"Method: {PaymentMethod ?? "N/A"}";
    }
}
