namespace LibraryManagementSystem.Models;

public class OverdueBooksReport
{
    public int BorrowingId { get; set; }

    public string MemberName { get; set; }
        = string.Empty;

    public string BookTitle { get; set; }
        = string.Empty;

    public DateTime DueDate { get; set; }

    public int DelayedDays { get; set; }

    public override string ToString()
    {
        return $"Borrowing Id: {BorrowingId} | " +
            $"Member: {MemberName} | Book: {BookTitle} | " +
            $"Due Date: {DueDate:d} | " +
            $"Delayed Days: {DelayedDays}";
    }
}
