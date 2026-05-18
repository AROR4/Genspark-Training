using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Interfaces;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly LibraryDbContext _context;

    public ReportRepository()
    {
        _context = new LibraryDbContext();
    }

    public Dictionary<
    string,
    List<AvailableBooksReport>>
    GetAvailableBooksByCategory()
    {
        try
        {
            List<AvailableBooksReport> books =
                _context.Database
                .SqlQueryRaw<
                    AvailableBooksReport>(
                    @"SELECT * FROM
                    get_available_books_report()")
                .ToList();

            return books
                .GroupBy(
                    b => b.CategoryName)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToList());
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while fetching available books report: "
                + ex.Message);
        }
    }

    public List<OverdueBooksReport>
    GetOverdueBooks()
    {
        try
        {
            return _context.Database
                .SqlQueryRaw<
                    OverdueBooksReport>(
                    @"SELECT * FROM
                    get_overdue_books_report()")
                .ToList();
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while fetching overdue books report: "
                + ex.Message);
        }
    }

    public List<MemberBorrowingSummaryReport>
    GetMemberBorrowingSummary(
        int memberId)
    {
        try
        {
            return _context.Database
                .SqlQueryRaw<
                    MemberBorrowingSummaryReport>(
                    @"SELECT * FROM
                    get_member_borrowing_summary({0})",
                    memberId)
                .ToList();
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while fetching member summary report: "
                + ex.Message);
        }
    }

    public List<PendingFineReport>
    GetMembersWithPendingFines()
    {
        try
        {
            return _context.Database
                .SqlQueryRaw<
                    PendingFineReport>(
                    @"SELECT * FROM
                    get_members_with_pending_fines()")
                .ToList();
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while fetching pending fines report: "
                + ex.Message);
        }
}

    public List<MostBorrowedBooksReport>
    GetMostBorrowedBooks()
    {
        try
        {
            return _context.Database
                .SqlQueryRaw<
                    MostBorrowedBooksReport>(
                    @"SELECT * FROM
                    get_most_borrowed_books()")
                .ToList();
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while fetching most borrowed books report: "
                + ex.Message);
        }
    }
}
