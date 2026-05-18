using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Interfaces;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Repositories;

public class FineRepository : IFineRepository
{
    private readonly LibraryDbContext _context;

    public FineRepository()
    {
        _context = new LibraryDbContext();
    }

    public void AddFine(Fine fine)
    {
        try
        {
            _context.Fines.Add(fine);
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while adding fine: "
                + ex.Message);
        }
    }

    public Fine? GetFineById(int fineId)
    {
        try
        {
            return _context.Fines
                .Include(f => f.Borrowing)
                .ThenInclude(b => b.Member)
                .Include(f => f.Borrowing)
                .ThenInclude(b => b.BookCopy)
                .ThenInclude(bc => bc.Book)
                .Include(f => f.FinePayments)
                .FirstOrDefault(
                    f => f.FineId == fineId);
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while fetching fine: "
                + ex.Message);
        }
    }

    public List<Fine> GetFinesByMember(int memberId)
    {
        try
        {
            return _context.Fines
                .Include(f => f.Borrowing)
                .ThenInclude(b => b.Member)
                .Include(f => f.Borrowing)
                .ThenInclude(b => b.BookCopy)
                .ThenInclude(bc => bc.Book)
                .Where(f =>
                    f.Borrowing.MemberId == memberId)
                .ToList();
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while fetching member fines: "
                + ex.Message);
        }
    }

    public decimal GetTotalUnpaidFine(int memberId)
    {
        try
        {
            return _context.Fines
                .Include(f => f.Borrowing)
                .ThenInclude(b => b.Member)
                .Include(f => f.Borrowing)
                .ThenInclude(b => b.BookCopy)
                .ThenInclude(bc => bc.Book)
                .Where(f =>
                    f.Borrowing.MemberId == memberId &&
                    !f.IsPaid)
                .Sum(f => f.FineAmount);
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while calculating unpaid fine: "
                + ex.Message);
        }
    }

    public void UpdateFine(Fine fine)
    {
        try
        {
            _context.Fines.Update(fine);
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while updating fine: "
                + ex.Message);
        }
    }

    public List<Fine> GetPendingFines(int memberId)
    {
        try
        {
            return _context.Fines
                .Include(f => f.Borrowing)
                .Where(f =>
                    f.Borrowing.MemberId == memberId &&
                    !f.IsPaid)
                .ToList();
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while fetching pending fines: "
                + ex.Message);
        }
    }


}
