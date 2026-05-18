using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Interfaces;

public interface IReportService
{
    public Dictionary<
    string,
    List<AvailableBooksReport>>
        GetAvailableBooksByCategory();

    List<OverdueBooksReport>
        GetOverdueBooks();

    List<MemberBorrowingSummaryReport>
        GetMemberBorrowingSummary(
            int memberId);

    List<PendingFineReport>
        GetMembersWithPendingFines();

    List<MostBorrowedBooksReport>
        GetMostBorrowedBooks();
}