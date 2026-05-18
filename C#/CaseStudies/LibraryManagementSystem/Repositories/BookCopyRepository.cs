using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Interfaces;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Repositories;

public class BookCopyRepository : IBookCopyRepository
{
    private readonly LibraryDbContext _context;

    public BookCopyRepository()
    {
        _context = new LibraryDbContext();
    }

    public void AddBookCopy(BookCopy bookCopy)
    {
        try
        {
            _context.BookCopies.Add(bookCopy);
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while adding book copy: "
                + ex.Message);
        }
    }

    public BookCopy? GetBookCopyById(int bookCopyId)
    {
        try
        {
            return _context.BookCopies
                .Include(bc => bc.Book)
                .FirstOrDefault(
                    bc => bc.BookCopyId == bookCopyId);
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while fetching book copy: "
                + ex.Message);
        }
    }

    public List<BookCopy> GetCopiesByBookId(int bookId)
    {
        try
        {
            return _context.BookCopies
                .Where(bc => bc.BookId == bookId)
                .ToList();
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while fetching copies: "
                + ex.Message);
        }
    }

    public List<BookCopy> GetAvailableCopiesByBookId(
        int bookId)
    {
        try
        {
            return _context.BookCopies
                .Where(bc =>
                    bc.BookId == bookId &&
                    bc.IsAvailable &&
                    bc.Status == "Available" &&
                    bc.DamagePercentage < 100)
                .ToList();
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while fetching available copies: "
                + ex.Message);
        }
    }

    public BookCopy? GetAvailableCopy(int bookId)
    {
        try
        {
            return _context.BookCopies
                .FirstOrDefault(bc =>
                    bc.BookId == bookId &&
                    bc.IsAvailable &&
                    bc.Status == "Available" &&
                    bc.DamagePercentage < 100);
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while fetching available copy: "
                + ex.Message);
        }
    }

    public void UpdateBookCopy(BookCopy bookCopy)
    {
        try
        {
            _context.BookCopies.Update(bookCopy);
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while updating book copy: "
                + ex.Message);
        }
    }

    public void MarkAsBorrowed(int bookCopyId)
    {
        try
        {
            BookCopy? copy = _context.BookCopies
                .FirstOrDefault(
                    bc => bc.BookCopyId == bookCopyId);

            if (copy == null)
            {
                throw new Exception(
                    "Book copy not found");
            }

            copy.IsAvailable = false;
            copy.Status = "Borrowed";

            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while marking copy borrowed: "
                + ex.Message);
        }
    }

    public void MarkAsAvailable(int bookCopyId)
    {
        try
        {
            BookCopy? copy = _context.BookCopies
                .FirstOrDefault(
                    bc => bc.BookCopyId == bookCopyId);

            if (copy == null)
            {
                throw new Exception(
                    "Book copy not found");
            }

            if (copy.DamagePercentage < 100)
            {
                copy.IsAvailable = true;
                copy.Status = "Available";
            }

            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while marking copy available: "
                + ex.Message);
        }
    }

    public void UpdateDamagePercentage(
        int bookCopyId,
        int damagePercentage)
    {
        try
        {
            BookCopy? copy = _context.BookCopies
                .FirstOrDefault(
                    bc => bc.BookCopyId == bookCopyId);

            if (copy == null)
            {
                throw new Exception(
                    "Book copy not found");
            }

            copy.DamagePercentage = damagePercentage;

            if (damagePercentage >= 100)
            {
                copy.IsAvailable = false;
                copy.Status = "Lost";
            }

            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while updating damage percentage: "
                + ex.Message);
        }
    }
}