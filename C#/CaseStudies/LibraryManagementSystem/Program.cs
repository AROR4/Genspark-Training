
using LibraryManagementSystem.Interfaces;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;
using System.Security.Cryptography;

namespace LibraryManagementSystem;

public class Program
{
    static readonly IAuthService _authService =
        new AuthService();

    static readonly IMemberService _memberService =
        new MemberService();

    static readonly IBookService _bookService =
        new BookService();

    static readonly IBorrowingService _borrowingService =
        new BorrowingService();

    static readonly IFineService _fineService =
        new FineService();

    static readonly IReportService _reportService =
        new ReportService();

    public static void Main(string[] args)
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine(
                "===== LIBRARY MANAGEMENT SYSTEM =====");

            Console.WriteLine("1. Login");
            Console.WriteLine("2. Exit");

            Console.Write("Enter choice: ");

            int choice;

            while (!int.TryParse(
                Console.ReadLine(),
                out choice))
            {
                Console.WriteLine(
                    "Please enter a valid number");

                Console.Write("Enter choice: ");
            }

            switch (choice)
            {
                case 1:
                    Login();
                    break;

                case 2:
                    return;

                default:
                    Console.WriteLine(
                        "Invalid choice");
                    break;
            }
        }
    }

    static void Login()
    {
        try
        {
            Console.Write("Enter Username: ");
            string username =
                Console.ReadLine()!;

            Console.Write("Enter Password: ");
            string password =
                Console.ReadLine()!;

            User? user =
                _authService.Login(
                    username,
                    password);

            if (user == null)
            {
                Console.WriteLine(
                    "Invalid login");

                return;
            }

            Console.WriteLine(
                $"Welcome {user.Username}");

            if (user.Role.Trim().ToLower().Equals(
                    "admin"))
            {
                AdminMenu();
            }
            else
            {
                UserMenu(user);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    static void AdminMenu()
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine(
                "===== ADMIN MENU =====");

            Console.WriteLine("1. Add Member");
            Console.WriteLine("2. View Members");
            Console.WriteLine("3. Search Member");
            Console.WriteLine("4. Update Membership Type");
            Console.WriteLine("5. Add Book");
            Console.WriteLine("6. Add Book Copy");
            Console.WriteLine("7. View Available Books");
            Console.WriteLine("8. Search Books");
            Console.WriteLine("9. Deactivate Member");
            Console.WriteLine("10. Activate Member");
            Console.WriteLine("11. Deactivate Book");
            Console.WriteLine("12. View Reports");
            Console.WriteLine("13. Logout");

            Console.Write("Enter choice: ");

            int choice;

            while (!int.TryParse(
                Console.ReadLine(),
                out choice))
            {
                Console.WriteLine(
                    "Please enter a valid number");

                Console.Write("Enter choice: ");
            }

            switch (choice)
            {
                case 1:
                    AddMember();
                    break;

                case 2:
                    ViewMembers();
                    break;

                case 3: 
                    SearchMembers();
                    break;
                
                case 4:
                    UpdateMemberMembership();
                    break;

                case 5:
                    AddBook();
                    break;

                case 6:
                    AddBookCopy();
                    break;

                case 7:
                    ViewAvailableBooks();
                    break;
                
                case 8: 
                    SearchBooks();
                    break; 

                case 9:
                    DeactivateMember();
                    break;

                case 10:
                    ActivateMember();
                    break;

                case 11:
                    DeactivateBook();
                    break;

                case 12:
                    AdminReportsMenu();
                    break;

                case 13:
                    return;

                default:
                    Console.WriteLine(
                        "Invalid choice");
                    break;
            }
        }
    }

    static void UserMenu(User user)
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("===== USER MENU =====");

            Console.WriteLine("1. View Available Books");
            Console.WriteLine("2. Search Books");
            Console.WriteLine("3. Borrow Book");
            Console.WriteLine("4. Return Book");
            Console.WriteLine("5. View Pending Fines");
            Console.WriteLine("6. View Fine History");
            Console.WriteLine("7. Pay Fine");
            Console.WriteLine("8. View Borrowed Book History");
            Console.WriteLine("9. Logout");

            Console.Write("Enter choice: ");

            int choice;

            while (!int.TryParse(
                Console.ReadLine(),
                out choice))
            {
                Console.WriteLine(
                    "Please enter a valid number");

                Console.Write("Enter choice: ");
            }

            switch (choice)
            {
                case 1:
                    ViewAvailableBooks();
                    break;

                case 2:
                    SearchBooks();
                    break;

                case 3:
                    BorrowBook(user);
                    break;

                case 4:
                    ReturnBook(user);
                    break;

                case 5:
                    ViewPendingFines(user);
                    break;

                case 6:
                    ViewFineHistory(user);
                    break;

                case 7:
                    PayFine(user);
                    break;

                case 8:
                    ViewBorrowedBookHistory(user);
                    break;

                case 9:
                    return;

                default:
                    Console.WriteLine(
                        "Invalid choice");
                    break;
            }
        }
    }

    static void AddMember()
    {
        try
        {
            Member member = new Member();

            Console.Write("Enter Full Name: ");
            member.FullName =Console.ReadLine()!;

            Console.Write("Enter Email: ");
            member.Email =Console.ReadLine()!;

            Console.Write("Enter Phone Number: ");
            member.PhoneNumber =Console.ReadLine()!;

            Console.Write("Enter Address: ");
            member.Address =Console.ReadLine();

            var membershipTypes =_memberService.GetAllMembershipTypes();

            if (membershipTypes.Count == 0)
            {
                Console.WriteLine(
                    "No membership types found. Please add membership types first.");

                return;
            }

            Console.WriteLine();
            Console.WriteLine("Available Membership Types:");

            foreach (var membershipType in membershipTypes)
            {
                Console.WriteLine(membershipType);
            }

            Console.Write("Enter Membership Type Id: ");

            int membershipTypeId;

            while (!int.TryParse(Console.ReadLine(),out membershipTypeId) ||
                !membershipTypes.Any(mt => mt.MembershipTypeId ==membershipTypeId))
            {
                Console.WriteLine(
                    "Please enter a valid Membership Type Id");

                Console.Write(
                    "Enter Membership Type Id: ");
            }

            member.MembershipTypeId =membershipTypeId;

            member.IsActive = true;

            _memberService.AddMember(member);

            string defaultPassword ="12345678";

            User user = new User
            {
                Username = member.Email,
                Password = defaultPassword,
                Role = "User",
                MemberId = member.MemberId
            };

            _authService.Register(user);

            Console.WriteLine(
                "Member added successfully");

            Console.WriteLine(
                "Member login created");

            Console.WriteLine(
                $"Username: {user.Username}");

            Console.WriteLine(
                $"Default Password: {defaultPassword}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    static void ViewMembers()
    {
        try
        {
            var members =_memberService.GetAllMembers();

            if (members.Count == 0)
            {
                Console.WriteLine("Sorry no members available");

                return;
            }

            foreach (var member in members)
            {
                Console.WriteLine(member);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    static void AddBook()
    {
        try
        {
            Book book = new Book();

            Console.Write("Title: ");
            book.Title =Console.ReadLine()!;

            Console.Write("Author: ");
            book.Author =Console.ReadLine()!;

            Console.Write("ISBN: ");
            book.ISBN =Console.ReadLine()!;

           Console.Write("Price: ");

            decimal price;

            while (!decimal.TryParse(
                Console.ReadLine(),
                out price) || price <= 0)
            {
                Console.WriteLine(
                    "Please enter a valid price");

                Console.Write("Price: ");
            }

            book.Price = price;

            Console.Write(
                "Published Year: ");

            Console.Write("Published Year: ");

            int publishedYear;

            while (!int.TryParse(
                Console.ReadLine(),
                out publishedYear))
            {
                Console.WriteLine(
                    "Please enter a valid year");

                Console.Write("Published Year: ");
            }

            book.PublishedYear = publishedYear;

            var categories =
            _bookService.GetAllCategories();

            if (categories.Count == 0)
            {
                Console.WriteLine(
                    "No categories found. Please add categories first.");

                return;
            }

            Console.WriteLine();
            Console.WriteLine(
                "Available Categories:");

            foreach (var category in categories)
            {
                Console.WriteLine(category);
            }

            Console.Write("Category Id: ");

            int categoryId;

            while (!int.TryParse(
                Console.ReadLine(),
                out categoryId) ||

                !categories.Any(
                    c => c.CategoryId == categoryId))
            {
                Console.WriteLine(
                    "Please enter a valid Category Id");

                Console.Write("Category Id: ");
            }

            book.CategoryId = categoryId;

            book.IsAvailable = true;

            _bookService.AddBook(book);

            Console.WriteLine(
                "Book added successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    static void AddBookCopy()
    {
        try
        {
            BookCopy copy = new BookCopy();

            var books =
                _bookService.GetAllBooks();

            if (books.Count == 0)
            {
                Console.WriteLine(
                    "No active books found. Please add a book first.");

                return;
            }

            Console.WriteLine();
            Console.WriteLine("Available Books:");

            foreach (var book in books)
            {
                Console.WriteLine(book);
            }

            Console.Write("Book Id: ");

            copy.BookId =
                Convert.ToInt32(
                    Console.ReadLine());

            Console.Write("Serial Number: ");

            copy.SerialNumber =
                Console.ReadLine()!;

            copy.IsAvailable = true;
            copy.Status = "Available";
            copy.DamagePercentage = 0;

            _bookService.AddBookCopy(copy);

            Console.WriteLine(
                "Book copy added successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    static void ViewAvailableBooks()
    {
        try
        {
            var books =
                _bookService
                .GetAvailableBooks();

            if (books.Count == 0)
            {
                Console.WriteLine(
                    "Sorry no books available");

                return;
            }

            foreach (var book in books)
            {
                Console.WriteLine(book);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

        static void SearchBooks()
        {
            try
            {
                while(true){

                Console.WriteLine();
                Console.WriteLine(
                    "===== SEARCH BOOKS =====");

                Console.WriteLine(
                    "1. Search By Title");

                Console.WriteLine(
                    "2. Search By Author");

                Console.WriteLine(
                    "3. Search By Category");
                
                Console.WriteLine(
                    "4. Exit"
                );

                Console.Write("Enter choice: ");

                int choice;

                while (!int.TryParse(
                    Console.ReadLine(),
                    out choice))
                {
                    Console.WriteLine(
                        "Please enter a valid choice");

                    Console.Write(
                        "Enter choice: ");
                }
                
                if (choice == 4)
                {
                    return;
                }
                List<Book> books =
                    new List<Book>();

                switch (choice)
                {
                    case 1:

                        Console.Write(
                            "Enter Title: ");

                        string title =
                            Console.ReadLine()!;

                        books =
                            _bookService
                            .SearchBooksByTitle(
                                title);

                        break;

                    case 2:

                        Console.Write(
                            "Enter Author: ");

                        string author =
                            Console.ReadLine()!;

                        books =
                            _bookService
                            .SearchBooksByAuthor(
                                author);

                        break;

                    case 3:

                        var categories =
                            _bookService
                            .GetAllCategories();

                        if (categories.Count == 0)
                        {
                            Console.WriteLine(
                                "No categories found");

                            break;
                        }

                        Console.WriteLine();
                        Console.WriteLine(
                            "Available Categories:");

                        foreach (var category in categories)
                        {
                            Console.WriteLine(
                                $"Id: {category.CategoryId} | " +
                                $"Name: {category.CategoryName}");
                        }

                        Console.Write(
                            "Enter Category Id: ");

                        int categoryId;

                        while (
                            !int.TryParse(
                                Console.ReadLine(),
                                out categoryId)

                            || !categories.Any(
                                c => c.CategoryId ==
                                    categoryId))
                        {
                            Console.WriteLine(
                                "Please enter a valid Category Id");

                            Console.Write(
                                "Enter Category Id: ");
                        }

                        books =
                            _bookService
                            .SearchBooksByCategory(
                                categoryId);

                        break;
                    
                    

                    default:

                        Console.WriteLine(
                            "Invalid choice");
                        return;
                }

                if (books.Count == 0)
                {
                    Console.WriteLine(
                        "No books found");

                    continue;
                }

                Console.WriteLine();
                Console.WriteLine(
                    "Books Found:");

                foreach (var book in books)
                {
                    int availableCopies =
                        book.BookCopies.Count(
                            bc =>
                                bc.IsAvailable &&
                                bc.Status == "Available" &&
                                bc.DamagePercentage < 100);

                    Console.WriteLine(
                        $"Book Id: {book.BookId} | " +
                        $"Title: {book.Title} | " +
                        $"Author: {book.Author} | " +
                        $"Category: {book.Category.CategoryName} | " +
                        $"Available Copies: {availableCopies}");
                }
                
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    static void SearchMembers()
    {
        try
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine(
                    "===== SEARCH MEMBERS =====");

                Console.WriteLine(
                    "1. Search By Email");

                Console.WriteLine(
                    "2. Search By Phone Number");

                Console.WriteLine(
                    "3. Exit");

                Console.Write("Enter choice: ");

                int choice;

                while (!int.TryParse(
                    Console.ReadLine(),
                    out choice))
                {
                    Console.WriteLine(
                        "Please enter a valid choice");

                    Console.Write(
                        "Enter choice: ");
                }

                if (choice == 3)
                {
                    return;
                }

                List<Member> members = new List<Member>();

                switch (choice)
                {
                    case 1:

                        Console.Write(
                            "Enter Email: ");

                        string email =
                            Console.ReadLine()!;

                        members =
                            _memberService
                            .SearchMembersByEmail(
                                email);

                        break;

                    case 2:

                        Console.Write(
                            "Enter Phone Number: ");

                        string phoneNumber =
                            Console.ReadLine()!;

                        members =
                            _memberService
                            .SearchMembersByPhoneNumber(
                                phoneNumber);

                        break;

                    default:

                        Console.WriteLine(
                            "Invalid choice");

                        continue;
                }

                if (members.Count==0)
                {
                    Console.WriteLine(
                        "No member found");

                    continue;
                }

                Console.WriteLine();
                Console.WriteLine(
                    "Member Found:");

                foreach (var member in members)
                {
                    Console.WriteLine(
                        $"Member Id: {member.MemberId} | " +
                        $"Name: {member.FullName} | " +
                        $"Email: {member.Email} | " +
                        $"Phone: {member.PhoneNumber} | " +
                        $"Membership: {member.MembershipType.Name} | " +
                        $"Status: {(member.IsActive ? "Active" : "Inactive")}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    static void DeactivateMember()
    {
        try
        {
            var members =_memberService.GetAllMembers();

            if (members.Count == 0)
            {
                Console.WriteLine(
                    "Sorry no active members available");

                return;
            }

            Console.WriteLine();
            Console.WriteLine("Active Members:");

            foreach (var member in members)
            {
                Console.WriteLine(member);
            }

            Console.Write("Member Id: ");

            int memberId;

            while (!int.TryParse(
                Console.ReadLine(),
                out memberId))
            {
                Console.WriteLine(
                    "Please enter a valid Member Id");

                Console.Write("Member Id: ");
            }

            if (!members.Any(
                    m => m.MemberId == memberId))
            {
                Console.WriteLine(
                    "Sorry no matching active member available");

                return;
            }

            _memberService
                .DeactivateMember(memberId);

            Console.WriteLine(
                "Member deactivated successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    static void ActivateMember()
    {
        try
        {
            var members =
                _memberService
                .GetInactiveMembers();

            if (members.Count == 0)
            {
                Console.WriteLine(
                    "Sorry no inactive members available");

                return;
            }

            Console.WriteLine();
            Console.WriteLine("Inactive Members:");

            foreach (var member in members)
            {
                Console.WriteLine(member);
            }

            Console.Write("Member Id: ");

            int memberId;

            while (!int.TryParse(
                Console.ReadLine(),
                out memberId))
            {
                Console.WriteLine(
                    "Please enter a valid Member Id");

                Console.Write("Member Id: ");
            }

            if (!members.Any(
                    m => m.MemberId == memberId))
            {
                Console.WriteLine(
                    "Sorry no matching inactive member available");

                return;
            }

            _memberService
                .ActivateMember(memberId);

            Console.WriteLine(
                "Member activated successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    static void DeactivateBook()
    {
        try
        {
            var books =_bookService.GetAllBooks();

            if (books.Count == 0)
            {
                Console.WriteLine(
                    "Sorry no active books available");

                return;
            }

            Console.WriteLine();
            Console.WriteLine("Active Books:");

            foreach (var book in books)
            {
                Console.WriteLine(book);
            }

            Console.Write("Book Id: ");

            Console.Write("Book Id: ");

            int bookId; 

            while (!int.TryParse(
                Console.ReadLine(),
                out bookId))
            {
                Console.WriteLine(
                    "Please enter a valid Book Id");

                Console.Write("Book Id: ");
            }
            if (!books.Any(
                    b => b.BookId == bookId))
            {
                Console.WriteLine(
                    "Sorry no matching active book available");

                return;
            }

            _bookService
                .DeactivateBook(bookId);

            Console.WriteLine(
                "Book deactivated successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    static void BorrowBook(User user)
    {
        try
        {
            var books =_bookService.GetAvailableBooks();

            if (books.Count == 0)
            {
                Console.WriteLine(
                    "Sorry no books available to borrow");

                return;
            }

            Console.WriteLine();
            Console.WriteLine("Available Books:");

            foreach (var book in books)
            {
                Console.WriteLine(book);
            }

            Console.Write("Book Id: ");

            int bookId;

            while (!int.TryParse(
                Console.ReadLine(),
                out bookId))
            {
                Console.WriteLine(
                    "Please enter a valid Book Id");

                Console.Write("Book Id: ");
            }

            if (!books.Any(
                    b => b.BookId == bookId))
            {
                Console.WriteLine(
                    "Sorry no matching active book available");

                return;
            }

            Borrowing borrowing =_borrowingService.BorrowBook(user.MemberId!.Value,bookId);

            Console.WriteLine(
                "Book borrowed successfully");
            
            Console.WriteLine(
                $"Book: {borrowing.BookCopy.Book.Title}");

            Console.WriteLine(
                $"Borrowing Id: {borrowing.BorrowingId}");

            Console.WriteLine(
                $"Borrow Date: {borrowing.BorrowDate:d}");

            Console.WriteLine(
                $"Due Date: {borrowing.DueDate:d}");

            Console.WriteLine(
                "Please keep the book safe and return it on or before the due date.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    static void ReturnBook(User user)
    {
        try
        {
            var borrowings =_borrowingService.GetActiveBorrowingsByMemberId(user.MemberId!.Value);

            if (borrowings.Count == 0)
            {
                Console.WriteLine(
                    "Sorry no borrowed books available to return");

                return;
            }

            Console.WriteLine();
            Console.WriteLine("Your Borrowed Books:");

            foreach (var borrowing in borrowings)
            {
                Console.WriteLine(borrowing);
            }

            Console.Write("Borrowing Id: ");

            int borrowingId;

            while (!int.TryParse(
                Console.ReadLine(),
                out borrowingId))
            {
                Console.WriteLine(
                    "Please enter a valid Borrowing Id");

                Console.Write("Borrowing Id: ");
            }

            if (!borrowings.Any(
                    b => b.BorrowingId == borrowingId))
            {
                Console.WriteLine(
                    "Sorry no matching borrowed book available");

                return;
            }

            Borrowing selectedBorrowing =
                borrowings.First(
                    b => b.BorrowingId == borrowingId);

            Console.Write(
                "Enter New Damage Percentage(for admin): ");

            int damage;

            while (!int.TryParse(
                Console.ReadLine(),
                out damage))
            {
                Console.WriteLine(
                    "Please enter a Valid Percetage");

                Console.Write("Enter Percentage ");
            }
            decimal fineAmount =_borrowingService.ReturnBook(borrowingId,damage);

            Console.WriteLine(
                "Book returned successfully");

            Console.WriteLine(
                $"Book: {selectedBorrowing.BookCopy.Book.Title}");

            Console.WriteLine(
                $"Borrowing Id: {selectedBorrowing.BorrowingId}");

            Console.WriteLine(
                $"Borrow Date: {selectedBorrowing.BorrowDate:d}");

            Console.WriteLine(
                $"Due Date: {selectedBorrowing.DueDate:d}");

            Console.WriteLine(
                $"Return Date: {DateTime.Now:d}");

            int delayedDays =
                Math.Max(
                    0,
                    (DateTime.Now.Date -
                     selectedBorrowing.DueDate.Date).Days);

            Console.WriteLine(
                $"Delayed Days: {delayedDays}");

            Console.WriteLine(
                $"Returned Damage Percentage: {damage}%");

            Console.WriteLine(
                $"Fine Amount: {fineAmount:C}");

            if (fineAmount > 0)
            {
                Console.WriteLine(
                    "Fine was added for late return or book damage.");
            }
            else
            {
                Console.WriteLine(
                    "No fine added. Thank you for returning the book safely.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    static void ViewPendingFines(User user)
    {
        try
        {
            var fines =_fineService.GetPendingFines(user.MemberId!.Value);

            if (fines.Count == 0)
            {
                Console.WriteLine(
                    "Sorry no pending fines available");

                return;
            }

            foreach (var fine in fines)
            {
                Console.WriteLine(fine);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    static void ViewFineHistory(User user)
    {
        try
        {
            var fines =_fineService.GetFineHistory(user.MemberId!.Value);

            if (fines.Count == 0)
            {
                Console.WriteLine(
                    "Sorry no fine history available");

                return;
            }

            foreach (var fine in fines)
            {
                Console.WriteLine(fine);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    static void PayFine(User user)
    {
        try
        {
            var fines =_fineService.GetPendingFines(user.MemberId!.Value);

            if (fines.Count == 0)
            {
                Console.WriteLine(
                    "Sorry no pending fines available to pay");

                return;
            }

            Console.WriteLine();
            Console.WriteLine("Pending Fines:");

            foreach (var fine in fines)
            {
                Console.WriteLine(fine);
            }

            Console.Write("Fine Id: ");

            int fineId;

            while (!int.TryParse(
                Console.ReadLine(),
                out fineId))
            {
                Console.WriteLine(
                    "Please enter a valid Fine Id");

                Console.Write("Fine Id: ");
            }

            if (!fines.Any(f => f.FineId == fineId))
            {
                Console.WriteLine(
                    "Sorry no matching pending fine available");

                return;
            }

            Console.Write(
                "Payment Method: ");

            string paymentMethod =
                Console.ReadLine()!;

            _fineService.PayFine(
                fineId,
                paymentMethod);

            Console.WriteLine(
                "Fine paid successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    static void viewActiveBorrowings(User user)
    {
        try
        {
            var borrowings =_borrowingService.GetActiveBorrowingsByMemberId(user.MemberId!.Value);

            if (borrowings.Count == 0)
            {
                Console.WriteLine(
                    "No Activ borrowed book !!");

                return;
            }

            foreach (var borrowing in borrowings)
            {
                Console.WriteLine(borrowing);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    static void ViewBorrowedBookHistory(User user)
    {
        try
        {
            var borrowings =_borrowingService.GetBorrowingHistoryByMemberId(user.MemberId!.Value);

            if (borrowings.Count == 0)
            {
                Console.WriteLine(
                    "No book borrowed yet history available");

                return;
            }

            foreach (var borrowing in borrowings)
            {
                Console.WriteLine(borrowing);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    static void UpdateMemberMembership()
    {
        try
        {
            var members =
                _memberService
                .GetAllMembers();

            if (members.Count == 0)
            {
                Console.WriteLine(
                    "No members found");

                return;
            }

            Console.WriteLine();
            Console.WriteLine(
                "Available Members:");

            foreach (var member in members)
            {
                Console.WriteLine(
                    $"Member Id: {member.MemberId} | " +
                    $"Name: {member.FullName} | " +
                    $"Current Membership: " +
                    $"{member.MembershipType.Name}");
            }

            Console.Write(
                "Enter Member Id: ");

            int memberId;

            while (
                !int.TryParse(
                    Console.ReadLine(),
                    out memberId)

                || !members.Any(
                    m => m.MemberId ==
                        memberId))
            {
                Console.WriteLine(
                    "Please enter a valid Member Id");

                Console.Write(
                    "Enter Member Id: ");
            }

            var membershipTypes =
                _memberService
                .GetAllMembershipTypes();

            if (membershipTypes.Count == 0)
            {
                Console.WriteLine(
                    "No membership types found");

                return;
            }

            Console.WriteLine();
            Console.WriteLine(
                "Available Membership Types:");

            foreach (var membershipType
                    in membershipTypes)
            {
                Console.WriteLine(
                    $"Id: {membershipType.MembershipTypeId} | " +
                    $"Name: {membershipType.Name} | " +
                    $"Max Books: {membershipType.MaxBorrowLimit} | " +
                    $"Max Days: {membershipType.MaxBorrowDays}");
            }

            Console.Write(
                "Enter Membership Type Id: ");

            int membershipTypeId;

            while (
                !int.TryParse(
                    Console.ReadLine(),
                    out membershipTypeId)

                || !membershipTypes.Any(
                    mt => mt.MembershipTypeId ==
                        membershipTypeId))
            {
                Console.WriteLine(
                    "Please enter a valid Membership Type Id");

                Console.Write(
                    "Enter Membership Type Id: ");
            }

            _memberService
                .UpdateMemberMembership(
                    memberId,
                    membershipTypeId);

            Console.WriteLine(
                "Membership updated successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    static void AdminReportsMenu()
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("===== REPORTS =====");
            Console.WriteLine("1. Books Currently Borrowed");
            Console.WriteLine("2. Overdue Books");
            Console.WriteLine("3. Members With Pending Fines");
            Console.WriteLine("4. Most Borrowed Books");
            Console.WriteLine("5. Available Books By Category");
            Console.WriteLine("6. Member Borrowing History");
            Console.WriteLine("7. Back");

            Console.Write("Enter choice: ");

            int choice;

            while (!int.TryParse(
                Console.ReadLine(),
                out choice))
            {
                Console.WriteLine(
                    "Please enter a valid choice");

                Console.Write("Enter choice: ");
            }

            switch (choice)
            {
                case 1:
                    ViewCurrentlyBorrowedBooksReport();
                    break;

                case 2:
                    ViewOverdueBooksReport();
                    break;

                case 3:
                    ViewMembersWithPendingFinesReport();
                    break;

                case 4:
                    ViewMostBorrowedBooksReport();
                    break;

                case 5:
                    ViewAvailableBooksByCategoryReport();
                    break;

                case 6:
                    ViewMemberBorrowingHistoryReport();
                    break;

                case 7:
                    return;

                default:
                    Console.WriteLine("Invalid choice");
                    break;
            }
        }
    }


    static void ViewCurrentlyBorrowedBooksReport()
    {
        try
        {
            var borrowings =_borrowingService.GetCurrentlyBorrowedBooks();

            PrintRecords(borrowings,"No Books are Borrowed !!");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    

    static void ViewOverdueBooksReport()
    {
        try
        {
            var reports =
                _reportService
                .GetOverdueBooks();

            PrintRecords(
                reports,
                "No books are overdued !!");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    

    static void ViewMembersWithPendingFinesReport()
    {
        try
        {
            var reports =
                _reportService
                .GetMembersWithPendingFines();

            PrintRecords(
                reports,
                "No members has pending fines !!");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    static void ViewMostBorrowedBooksReport()
    {
        try
        {
            var reports =
                _reportService
                .GetMostBorrowedBooks();

            PrintRecords(
                reports,
                "No books are  borrowed yet !!");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    static void ViewAvailableBooksByCategoryReport()
    {
        try
        {
            var reports =
                _reportService
                .GetAvailableBooksByCategory();

            if (reports.Count == 0)
            {
                Console.WriteLine(
                    "No books are available in the library !!");
                return;
            }

            foreach (var category in reports)
            {
                Console.WriteLine();
                Console.WriteLine(
                    $"===== {category.Key} =====");

                foreach (var book in category.Value)
                {
                    Console.WriteLine(
                        $"Book Id: {book.BookId} | " +
                        $"Title: {book.Title} | " +
                        $"Author: {book.Author} | " +
                        $"Available Copies: {book.AvailableCopies}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    static void ViewMemberBorrowingHistoryReport()
    {
        try
        {
            var members =
                _memberService.GetAllMembers();

            if (members.Count == 0)
            {
                Console.WriteLine(
                    "Sorry no active members available");

                return;
            }

            Console.WriteLine();
            Console.WriteLine("Active Members:");

            foreach (var member in members)
            {
                Console.WriteLine(member);
            }

            Console.Write("Member Id: ");

            int memberId;

            while (!int.TryParse(
                Console.ReadLine(),
                out memberId))
            {
                Console.WriteLine(
                    "Please enter a valid Member Id");

                Console.Write("Member Id: ");
            }

            if (!members.Any(
                    m => m.MemberId == memberId))
            {
                Console.WriteLine(
                    "Sorry no matching active member available");

                return;
            }

            var borrowings =
                _borrowingService
                .GetBorrowingHistoryByMemberId(
                    memberId);

            PrintRecords(
                borrowings,
                "Sorry no member borrowing history available");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    static void PrintRecords<T>(
        List<T> records,
        string emptyMessage)
    {
        if (records.Count == 0)
        {
            Console.WriteLine(emptyMessage);
            return;
        }

        foreach (var record in records)
        {
            Console.WriteLine(record);
        }
    }
}
