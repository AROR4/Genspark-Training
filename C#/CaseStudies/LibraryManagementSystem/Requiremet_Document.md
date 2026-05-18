Requirement Understanding Document
Library Management & Book Lending System

1. Project Overview
The Library Management & Book Lending System is a .NET Core Console Application developed using .NET Core, Entity Framework Core, and PostgreSQL. The system automates the operations of a small community library.

2. Objectives
- Manage library members
- Maintain books and copies
- Automate borrowing and returns
- Calculate fines
- Generate reports
- Implement authentication and authorization

3. Technologies Used
- .NET Core
- C#
- Entity Framework Core
- PostgreSQL
- Npgsql

4. Functional Requirements

4.1 Authentication Module
Roles:
- Admin/Librarian
- User/Member

Admin can:
- Add members
- Deactivate members
- Add books and copies
- View reports
- Manage fines

Users can:
- View books
- Borrow books
- Return books
- View and pay fines

4.2 Member Management
- Add member
- View all members
- Search member
- Update status
- Deactivate member

Membership Types:
- Basic
- Student
- Premium

4.3 Membership Rules

Basic:
- Max Books: 2
- Max Days: 7

Student:
- Max Books: 3
- Max Days: 10

Premium:
- Max Books: 5
- Max Days: 15

4.4 Book Management
- Add books
- Add copies
- View available books
- Search books
- Mark damaged/lost books

4.5 Borrowing Rules
Borrowing blocked if:
- Member inactive
- Borrow limit exceeded
- Pending fines above ₹500
- No available copies
- Duplicate borrowing

4.6 Return Rules
- Return date updated
- Late fine calculated
- Damage fine calculated
- Book copy updated
- Fine record created

4.7 Fine Management
- View pending fines
- Pay fine
- View fine history

4.8 Reports
- Available books by category
- Overdue books
- Most borrowed books
- Pending fines
- Member borrowing summary

5. Database Design

Main Entities:
- User
- Member
- MembershipType
- Category
- Book
- BookCopy
- Borrowing
- Fine
- FinePayment

Relationships:
- MembershipType → Members
- Category → Books
- Book → BookCopies
- Member → Borrowings
- Borrowing → Fine

6. Business Rules
- Only active members can borrow
- Fine above ₹500 blocks borrowing
- Fully damaged books marked lost
- Borrowing transactions use rollback

7. PostgreSQL Functions and Procedures
Functions:
- get_available_books_report()
- get_overdue_books_report()
- get_member_borrowing_summary()
- get_members_with_pending_fines()
- get_most_borrowed_books()

Procedure/Function:
- process_book_return()

8. Transaction Management
Transactions are implemented during:
- Borrow book
- Return book

Rollback occurs if any step fails.

9. Exception Handling
Custom validations:
- Invalid member
- Invalid book
- No available copies
- Fine limit exceeded
- Borrowing limit exceeded
- Duplicate borrowing

10. Conclusion
The project demonstrates real-world implementation of EF Core, PostgreSQL, layered architecture, transaction handling, reporting, and business rule enforcement in a library management system.
