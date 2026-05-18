using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Interfaces;

public interface IFineService
{
    List<Fine> GetPendingFines(int memberId);

    List<Fine> GetFineHistory(int memberId);

    void PayFine(
        int fineId,
        string paymentMethod);
}