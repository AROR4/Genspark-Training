using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Interfaces;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Repositories;

public class BorrowingRepository : IBorrowingRepository
{
    private readonly LibraryDbContext _context;

    public BorrowingRepository()
    {
        _context = new LibraryDbContext();
    }

    public void AddBorrowing(Borrowing borrowing)
    {
        _context.Borrowings.Add(borrowing);
        _context.SaveChanges();
    }

    public Borrowing? GetBorrowingById(int borrowingId)
    {
        return _context.Borrowings
            .Include(b => b.Member)
            .Include(b => b.BookCopy)
            .ThenInclude(bc => bc.Book)
            .FirstOrDefault(b => b.BorrowingId == borrowingId);
    }

    public List<Borrowing> GetAllBorrowings()
    {
        return _context.Borrowings
            .Include(b => b.Member)
            .Include(b => b.BookCopy)
            .ThenInclude(bc => bc.Book)
            .OrderByDescending(b => b.BorrowDate)
            .ToList();
    }

    public List<Borrowing> GetBorrowingsByMemberId(int memberId)
    {
        return _context.Borrowings
            .Include(b => b.Member)
            .Include(b => b.BookCopy)
            .ThenInclude(bc => bc.Book)
            .Where(b => b.MemberId == memberId)
            .OrderByDescending(b => b.BorrowDate)
            .ToList();
    }

    public List<Borrowing> GetActiveBorrowingsByMemberId(int memberId)
    {
        return _context.Borrowings
            .Include(b => b.Member)
            .Include(b => b.BookCopy)
            .ThenInclude(bc => bc.Book)
            .Where(b =>
                b.MemberId == memberId &&
                b.ReturnDate == null)
            .OrderByDescending(b => b.BorrowDate)
            .ToList();
    }

    public int GetActiveBorrowingCount(int memberId)
    {
        return _context.Borrowings
            .Count(b =>
                b.MemberId == memberId &&
                b.ReturnDate == null);
    }

    public bool HasActiveBorrowingForBook(int memberId, int bookId)
    {
        return _context.Borrowings
            .Include(b => b.BookCopy)
            .Any(b =>
                b.MemberId == memberId &&
                b.BookCopy.BookId == bookId &&
                b.ReturnDate == null);
    }

    public Borrowing? GetActiveBorrowingByCopyId(int bookCopyId)
    {
        return _context.Borrowings
            .FirstOrDefault(b =>
                b.BookCopyId == bookCopyId &&
                b.ReturnDate == null);
    }

    public List<Borrowing> GetOverdueBorrowings()
    {
        return _context.Borrowings
            .Include(b => b.Member)
            .Include(b => b.BookCopy)
            .ThenInclude(bc => bc.Book)
            .Where(b =>
                b.ReturnDate == null &&
                b.DueDate < DateTime.Now)
            .ToList();
    }

    public void UpdateBorrowing(Borrowing borrowing)
    {
        _context.Borrowings.Update(borrowing);
        _context.SaveChanges();
    }


    public decimal ProcessBookReturn(
    int borrowingId,
    int newDamagePercentage)
    {
        try
        {
            var result = _context.Database
                .SqlQuery<decimal>(
                    $"SELECT process_book_return({borrowingId}, {newDamagePercentage})")
                .AsEnumerable()
                .FirstOrDefault();

            return result;
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while processing return: "
            + ex.Message);
        }
    }   
}
