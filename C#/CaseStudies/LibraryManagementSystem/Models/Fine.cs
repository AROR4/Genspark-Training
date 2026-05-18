using System.Collections.Generic;

namespace LibraryManagementSystem.Models;

public class Fine
{
    public int FineId { get; set; }

    public int BorrowingId { get; set; }

    public decimal FineAmount { get; set; }

    public bool IsPaid { get; set; } = false;

    public Borrowing Borrowing { get; set; } = null!;

    public ICollection<FinePayment> FinePayments { get; set; } = new List<FinePayment>();

    public override string ToString()
    {
        string memberName =
            Borrowing?.Member?.FullName ??
            Borrowing?.MemberId.ToString() ??
            "N/A";

        string bookTitle =
            Borrowing?.BookCopy?.Book?.Title ??
            BorrowingId.ToString();

        return $"Id: {FineId} | Borrowing Id: {BorrowingId} | " +
            $"Member: {memberName} | Book: {bookTitle} | " +
            $"Amount: {FineAmount:C} | Paid: {IsPaid}";
    }
}
