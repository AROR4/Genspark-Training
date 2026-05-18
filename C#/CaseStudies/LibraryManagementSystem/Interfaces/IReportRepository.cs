using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Interfaces;

public interface IReportRepository
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