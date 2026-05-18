using LibraryManagementSystem.Interfaces;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories;

namespace LibraryManagementSystem.Services;

public class ReportService : IReportService
{
    private readonly IReportRepository
        _reportRepository;

    public ReportService()
    {
        _reportRepository =
            new ReportRepository();
    }

    public Dictionary<
    string,
    List<AvailableBooksReport>>
        GetAvailableBooksByCategory()
    {
        try
        {
            return _reportRepository
                .GetAvailableBooksByCategory();
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
            return _reportRepository
                .GetOverdueBooks();
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
            return _reportRepository
                .GetMemberBorrowingSummary(
                    memberId);
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
            return _reportRepository
                .GetMembersWithPendingFines();
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
            return _reportRepository
                .GetMostBorrowedBooks();
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while fetching most borrowed books report: "
                + ex.Message);
        }
    }
}