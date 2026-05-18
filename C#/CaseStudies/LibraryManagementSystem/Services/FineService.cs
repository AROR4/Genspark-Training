using LibraryManagementSystem.Interfaces;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories;

namespace LibraryManagementSystem.Services;

public class FineService : IFineService
{
    private readonly IFineRepository _fineRepository;

    private readonly LibraryDbContext _context;

    public FineService()
    {
        _fineRepository = new FineRepository();

        _context = new LibraryDbContext();
    }

    public List<Fine> GetPendingFines(
        int memberId)
    {
        try
        {
            return _fineRepository
                .GetPendingFines(memberId);
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while fetching pending fines: "
                + ex.Message);
        }
    }

    public List<Fine> GetFineHistory(
        int memberId)
    {
        try
        {
            return _fineRepository
                .GetFinesByMember(memberId);
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while fetching fine history: "
                + ex.Message);
        }
    }

    public void PayFine(
        int fineId,
        string paymentMethod)
    {
        using var transaction =
            _context.Database.BeginTransaction();

        try
        {
            Fine? fine =
                _fineRepository
                .GetFineById(fineId);

            if (fine == null)
            {
                throw new Exception(
                    "Fine not found");
            }

            if (fine.IsPaid)
            {
                throw new Exception(
                    "Fine already paid");
            }

            // Create Payment Record

            FinePayment payment =
                new FinePayment
                {
                    FineId = fineId,

                    AmountPaid =
                        fine.FineAmount,

                    PaymentDate =
                        DateTime.Now,

                    PaymentMethod =
                        paymentMethod
                };

            _context.FinePayments
                .Add(payment);

            // Update Fine

            fine.IsPaid = true;

            _fineRepository
                .UpdateFine(fine);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}