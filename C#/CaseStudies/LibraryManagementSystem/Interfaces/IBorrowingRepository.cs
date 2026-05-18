using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Interfaces;

public interface IBorrowingRepository
{
    void AddBorrowing(Borrowing borrowing);

    Borrowing? GetBorrowingById(int borrowingId);

    List<Borrowing> GetAllBorrowings();

    List<Borrowing> GetBorrowingsByMemberId(int memberId);

    List<Borrowing> GetActiveBorrowingsByMemberId(int memberId);

    int GetActiveBorrowingCount(int memberId);

    bool HasActiveBorrowingForBook(int memberId, int bookId);

    Borrowing? GetActiveBorrowingByCopyId(int bookCopyId);

    List<Borrowing> GetOverdueBorrowings();

    void UpdateBorrowing(Borrowing borrowing);

    decimal ProcessBookReturn(
    int borrowingId,
    int newDamagePercentage);
}