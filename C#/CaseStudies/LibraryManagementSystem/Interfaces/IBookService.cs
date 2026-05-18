using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Interfaces;

public interface IBookService
{
    void AddBook(Book book);

    void AddBookCopy(BookCopy bookCopy);

    List<Book> GetAllBooks();

    List<Book> GetAvailableBooks();

    List<Category> GetAllCategories();

    Book? GetBookById(int bookId);

    List<Book> SearchBooksByTitle(string title);

    List<Book> SearchBooksByAuthor(string author);

    List<Book> SearchBooksByCategory(int categoryId);

    void DeactivateBook(int bookId);

    void UpdateDamagePercentage(
        int bookCopyId,
        int damagePercentage);
}
