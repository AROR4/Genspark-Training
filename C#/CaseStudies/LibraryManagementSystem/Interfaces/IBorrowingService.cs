using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Interfaces;

public interface IBorrowingService
{
    Borrowing BorrowBook(
        int memberId,
        int bookId);

    decimal ReturnBook(
        int borrowingId,
        int newDamagePercentage);

    List<Borrowing> GetActiveBorrowingsByMemberId(
        int memberId);

    List<Borrowing> GetBorrowingHistoryByMemberId(
        int memberId);

    List<Borrowing> GetCurrentlyBorrowedBooks();
}
