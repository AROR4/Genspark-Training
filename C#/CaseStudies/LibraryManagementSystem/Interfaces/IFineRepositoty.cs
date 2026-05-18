using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Interfaces;

public interface IFineRepository
{
    void AddFine(Fine fine);

    Fine? GetFineById(int fineId);

    List<Fine> GetFinesByMember(int memberId);

    decimal GetTotalUnpaidFine(int memberId);

    void UpdateFine(Fine fine);
    List<Fine> GetPendingFines(int memberId);
}