using LibraryManagementSystem.Interfaces;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories;

namespace LibraryManagementSystem.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;
    private readonly IBookCopyRepository _bookCopyRepository;

    public BookService()
    {
        _bookRepository = new BookRepository();
        _bookCopyRepository = new BookCopyRepository();
    }

    public void AddBook(Book book)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(book.Title))
            {
                throw new Exception(
                    "Book title cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(book.Author))
            {
                throw new Exception(
                    "Author name cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(book.ISBN))
            {
                throw new Exception(
                    "ISBN cannot be empty");
            }

            if (book.Price <= 0)
            {
                throw new Exception(
                    "Book price must be greater than 0");
            }

            _bookRepository.AddBook(book);
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while adding book: "
                + ex.Message);
        }
    }

    public void AddBookCopy(BookCopy bookCopy)
    {
        try
        {
            Book? book =
                _bookRepository.GetBookById(
                    bookCopy.BookId);

            if (book == null)
            {
                throw new Exception(
                    "Book not found");
            }

            if (!book.IsAvailable)
            {
                throw new Exception(
                    "Book is inactive");
            }

            if (string.IsNullOrWhiteSpace(
                bookCopy.SerialNumber))
            {
                throw new Exception(
                    "Serial number cannot be empty");
            }

            _bookCopyRepository
                .AddBookCopy(bookCopy);
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while adding book copy: "
                + ex.Message);
        }
    }

    public List<Book> GetAllBooks()
    {
        try
        {
            return _bookRepository
                .GetAllBooks();
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while fetching books: "
                + ex.Message);
        }
    }

    public List<Book> GetAvailableBooks()
    {
        try
        {
            return _bookRepository
                .GetAvailableBooks();
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while fetching available books: "
                + ex.Message);
        }
    }

    public List<Category> GetAllCategories()
    {
        try
        {
            return _bookRepository
                .GetAllCategories();
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while fetching categories: "
                + ex.Message);
        }
    }

    public Book? GetBookById(int bookId)
    {
        try
        {
            return _bookRepository
                .GetBookById(bookId);
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while fetching book: "
                + ex.Message);
        }
    }

    public List<Book> SearchBooksByTitle(string title)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new Exception(
                    "Title cannot be empty");
            }

            return _bookRepository
                .SearchBooksByTitle(title);
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while searching books by title: "
                + ex.Message);
        }
    }

    public List<Book> SearchBooksByAuthor(string author)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(author))
            {
                throw new Exception(
                    "Author cannot be empty");
            }

            return _bookRepository
                .SearchBooksByAuthor(author);
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while searching books by author: "
                + ex.Message);
        }
    }

    public List<Book> SearchBooksByCategory(int categoryId)
    {
        try
        {
            return _bookRepository
                .SearchBooksByCategory(categoryId);
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while searching books by category: "
                + ex.Message);
        }
    }

    public void DeactivateBook(int bookId)
    {
        try
        {
            Book? book =
                _bookRepository
                .GetBookById(bookId);

            if (book == null)
            {
                throw new Exception(
                    "Book not found");
            }

            _bookRepository
                .DeactivateBook(bookId);
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while deactivating book: "
                + ex.Message);
        }
    }

    public void UpdateDamagePercentage(
        int bookCopyId,
        int damagePercentage)
    {
        try
        {
            if (damagePercentage < 0 ||
                damagePercentage > 100)
            {
                throw new Exception(
                    "Damage percentage must be between 0 and 100");
            }

            BookCopy? copy =
                _bookCopyRepository
                .GetBookCopyById(bookCopyId);

            if (copy == null)
            {
                throw new Exception(
                    "Book copy not found");
            }

            _bookCopyRepository
                .UpdateDamagePercentage(
                    bookCopyId,
                    damagePercentage);
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while updating damage percentage: "
                + ex.Message);
        }
    }
}
