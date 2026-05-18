using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Interfaces;

public interface IBookCopyRepository
{
    void AddBookCopy(BookCopy bookCopy);

    BookCopy? GetBookCopyById(int bookCopyId);

    List<BookCopy> GetCopiesByBookId(int bookId);

    List<BookCopy> GetAvailableCopiesByBookId(int bookId);

    BookCopy? GetAvailableCopy(int bookId);

    void UpdateBookCopy(BookCopy bookCopy);

    void MarkAsBorrowed(int bookCopyId);

    void MarkAsAvailable(int bookCopyId);

    void UpdateDamagePercentage(
        int bookCopyId,
        int damagePercentage);
}