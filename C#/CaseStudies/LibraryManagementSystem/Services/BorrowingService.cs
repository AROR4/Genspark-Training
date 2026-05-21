using LibraryManagementSystem.Interfaces;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories;
using LibraryManagementSystem.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Services
{
    public class BorrowingService : IBorrowingService
    {
        private readonly LibraryDbContext _context;
        private readonly IMemberRepository _memberRepository;
        private readonly IBorrowingRepository _borrowingRepository;
        private readonly IFineRepository _fineRepository;
        private readonly IBookCopyRepository _bookCopyRepository;
        

        public BorrowingService(LibraryDbContext context)
        {
            _context = context;
            _memberRepository=new MemberRepository(context);
            _borrowingRepository=new BorrowingRepository(context);
            _fineRepository=new FineRepository(context);
            _bookCopyRepository=new BookCopyRepository(context);
        }
        public Borrowing BorrowBook(int memberId, int bookId)
        {
            using var transaction =
                _context.Database.BeginTransaction();

            try
            {
                // 1. Validate Member

                Member? member =
                    _memberRepository
                    .GetMemberById(memberId);

                if (member == null)
                {
                    throw new Exception(
                        "Member not found");
                }

                if (!member.IsActive)
                {
                    throw new Exception(
                        "Member is inactive");
                }

                // 2. Check Unpaid Fine

                decimal totalFine =
                    _fineRepository
                    .GetTotalUnpaidFine(memberId);

                if (totalFine > 500)
                {
                    throw new FineLimitExceededException(
                        "Pending fine exceeds ₹500");
                }

                // 3. Check Borrow Limit

                List<Borrowing> activeBorrowings =
                    _borrowingRepository
                    .GetActiveBorrowingsByMemberId(
                        memberId);

                if (activeBorrowings.Count >=
                    member.MembershipType.MaxBorrowLimit)
                {
                    throw new BorrowLimitExceededException(
                        $"Borrowing limit exceeded for {member.MembershipType.Name} membership \n Active Borrowings : {member.MembershipType.MaxBorrowLimit} . Return Book to Borrow");
                }

                // 4. Check Duplicate Borrowing

                bool alreadyBorrowed =
                    activeBorrowings.Any(b =>
                        b.BookCopy.BookId == bookId);

                if (alreadyBorrowed)
                {
                    throw new DuplicateBorrowingException(
                        "Member already borrowed this book");
                }

                // 5. Get Available Copy

                BookCopy? availableCopy =
                    _bookCopyRepository
                    .GetAvailableCopyForBorrowing(bookId);

                if (availableCopy == null)
                {
                    throw new BookUnavailableException(
                        "No available copy found");
                }

                // 6. Create Borrowing

                Borrowing borrowing = new Borrowing
                {
                    MemberId = memberId,
                    BookCopyId = availableCopy.BookCopyId,
                    BorrowDate = DateTime.Now,
                    OldDamagePercentage =
                        availableCopy.DamagePercentage,
                    NewDamagePercentage = null,
                    FineAmount = 0,

                    DueDate = DateTime.Today.AddDays(
                        member.MembershipType.MaxBorrowDays),

                    Status = "Borrowed"
                };

                _borrowingRepository
                    .AddBorrowing(borrowing);

                // 7. Update Book Copy

                _bookCopyRepository
                    .MarkAsBorrowed(
                        availableCopy.BookCopyId);

                // 8. Commit Transaction

                transaction.Commit();

                Borrowing? savedBorrowing =
                    _borrowingRepository
                    .GetBorrowingById(
                        borrowing.BorrowingId);

                return savedBorrowing ?? borrowing;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public Borrowing ReturnBook(
            int borrowingId,
            int newDamagePercentage)
        {
            using var transaction =
                _context.Database.BeginTransaction();

            try
            {
                Borrowing? borrowing =
                    _borrowingRepository
                    .GetBorrowingById(
                        borrowingId);

                if (borrowing == null)
                {
                    throw new Exception(
                        "Borrowing record not found");
                }

                if (!borrowing.Status.Equals("Borrowed"))
                {
                    throw new Exception(
                        "Book already returned");
                }

                if (newDamagePercentage < 0 ||
                    newDamagePercentage > 100)
                {
                    throw new Exception(
                        "Damage percentage must be between 0 and 100");
                }

                int currentDamagePercentage =
                    borrowing.BookCopy.DamagePercentage;

                if (newDamagePercentage <
                    currentDamagePercentage)
                {
                    throw new Exception(
                        "Return damage percentage cannot be less than current damage percentage");
                }

                decimal totalFine =
                    _borrowingRepository
                    .ProcessBookReturn(
                        borrowing.BorrowingId,
                        newDamagePercentage);

                //  Update Borrowing


                borrowing.ReturnDate = DateTime.Now;

                borrowing.Status =
                    "Returned";
                

                //  Update BookCopy

                borrowing.BookCopy.DamagePercentage =
                    newDamagePercentage;
                
                borrowing.NewDamagePercentage = newDamagePercentage;
                if (newDamagePercentage >= 100)
                {
                    borrowing.BookCopy.Status =
                        "Lost";

                    borrowing.BookCopy.IsAvailable =
                        false;
                }
                else
                {
                    borrowing.BookCopy.Status =
                        "Available";

                    borrowing.BookCopy.IsAvailable =
                        true;
                }

                //  Create Fine

                if (totalFine > 0)
                {
                    Fine fine = new Fine
                    {
                        BorrowingId = borrowingId,
                        FineAmount = totalFine,
                        IsPaid = false,
                    };
                    borrowing.FineAmount = totalFine;
                    _fineRepository.AddFine(fine);
                }

                _context.SaveChanges();

                transaction.Commit();

                return borrowing;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception(
                    "Error while processing book return: "
                    + ex.Message);
            }
        }

        public List<Borrowing> GetActiveBorrowingsByMemberId(
            int memberId)
        {
            try
            {
                return _borrowingRepository
                    .GetActiveBorrowingsByMemberId(
                        memberId);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error while fetching active borrowings: "
                    + ex.Message);
            }
        }

        public List<Borrowing> GetBorrowingHistoryByMemberId(
            int memberId)
        {
            try
            {
                return _borrowingRepository
                    .GetBorrowingsByMemberId(
                        memberId);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error while fetching borrowing history: "
                    + ex.Message);
            }
        }

        public List<Borrowing> GetCurrentlyBorrowedBooks()
        {
            try
            {
                return _borrowingRepository
                    .GetAllBorrowings()
                    .Where(b => b.ReturnDate == null)
                    .OrderByDescending(b => b.BorrowDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error while fetching currently borrowed books: "
                    + ex.Message);
            }
        }
    }

    
}
