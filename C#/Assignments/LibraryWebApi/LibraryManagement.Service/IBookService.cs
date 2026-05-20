using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Services
{
    public interface IBookService
    {
        Book AddBook(BookDTO book);
        List<Book> GetAllBooks();
        Book GetBookById(int id);
        List<Book> SearchBooks(string title);
    }
}