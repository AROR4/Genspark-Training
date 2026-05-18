namespace LibraryManagementSystem.Models;

public class MemberBorrowingSummaryReport
{
    public long TotalBorrowedBooks { get; set; }

    public long ReturnedBooks { get; set; }

    public decimal PendingFine { get; set; }

    public override string ToString()
    {
        return $"Total Borrowed Books: {TotalBorrowedBooks} | " +
            $"Returned Books: {ReturnedBooks} | " +
            $"Pending Fine: {PendingFine:C}";
    }
}
