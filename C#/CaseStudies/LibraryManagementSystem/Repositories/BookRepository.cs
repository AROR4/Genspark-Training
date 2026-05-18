using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Interfaces;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Repositories;

public class BookRepository : IBookRepository
{
    private readonly LibraryDbContext _context;

    public BookRepository()
    {
        _context = new LibraryDbContext();
    }

    public void AddBook(Book book)
    {
        try
        {
            _context.Books.Add(book);
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while adding book: " + ex.InnerException?.Message);
        }
    }

    public List<Book> GetAllBooks()
    {
        try
        {
            return _context.Books
                .Include(b => b.Category)
                .Include(b => b.BookCopies)
                .Where(b => b.IsAvailable)
                .ToList();
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while fetching books: " + ex.Message);
        }
    }

    public Book? GetBookById(int bookId)
    {
        try
        {
            return _context.Books
                .Include(b => b.Category)
                .Include(b => b.BookCopies)
                .FirstOrDefault(b => b.BookId == bookId);
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while fetching book: " + ex.Message);
        }
    }

    public List<Book> SearchBooksByTitle(string title)
    {
        try
        {
            return _context.Books
                .Include(b => b.Category)
                .Where(b =>
                    b.IsAvailable &&
                    b.Title.ToLower()
                    .Contains(title.ToLower()))
                .ToList();
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while searching books by title: " + ex.Message);
        }
    }

    public List<Book> SearchBooksByAuthor(string author)
    {
        try
        {
            return _context.Books
                .Include(b => b.Category)
                .Where(b =>
                    b.IsAvailable &&
                    b.Author.ToLower()
                    .Contains(author.ToLower()))
                .ToList();
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while searching books by author: " + ex.Message);
        }
    }

    public List<Book> SearchBooksByCategory(int categoryId)
    {
        try
        {
            return _context.Books
                .Include(b => b.Category)
                .Where(b =>
                    b.IsAvailable &&
                    b.CategoryId == categoryId)
                .ToList();
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while searching books by category: " + ex.Message);
        }
    }

    public void UpdateBook(Book book)
    {
        try
        {
            _context.Books.Update(book);
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while updating book: " + ex.Message);
        }
    }

    public void DeactivateBook(int bookId)
    {
        try
        {
            Book? book = _context.Books
                .FirstOrDefault(b => b.BookId == bookId);

            if (book != null)
            {
                book.IsAvailable = false;
                _context.SaveChanges();
            }
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while deactivating book: " + ex.Message);
        }
    }

    public List<Book> GetAvailableBooks()
    {
        try
        {
            return _context.Books
                .Include(b => b.BookCopies)
                .Include(b => b.Category)
                .Where(b =>
                    b.IsAvailable &&
                    b.BookCopies.Any(
                        bc => bc.IsAvailable))
                .ToList();
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while fetching available books: " + ex.Message);
        }
    }

    public List<Category> GetAllCategories()
    {
        try
        {
            return _context.Categories
                .OrderBy(c => c.CategoryId)
                .ToList();
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while fetching categories: " + ex.Message);
        }
    }
}
