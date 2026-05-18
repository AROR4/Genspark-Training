using LibraryManagementSystem.Models;
namespace LibraryManagementSystem.Interfaces;

public interface IBookRepository
{
    void AddBook(Book book);

    List<Book> GetAllBooks();

    Book? GetBookById(int bookId);

    List<Book> SearchBooksByTitle(string title);

    List<Book> SearchBooksByAuthor(string author);

    List<Book> SearchBooksByCategory(int categoryId);

    void UpdateBook(Book book);

    void DeactivateBook(int bookId);

    List<Book> GetAvailableBooks();

    List<Category> GetAllCategories();
}
