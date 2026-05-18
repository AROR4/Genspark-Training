Main Entities

Member
MembershipType
Book
Category
BookCopy
Borrowing
Fine
FinePayment



Tables Design

1. MembershipTypes

MembershipTypeId
Name
MaxBorrowLimit
MaxBorrowDays

2. Members

MemberId
FullName
Email
PhoneNumber
Address
MembershipTypeId
IsActive
JoinedDate
CreatedAt

3. Categories

CategoryId
CategoryName
Description

4. Books

BookId
Title
Author
ISBN
PublishedYear
CategoryId
Description
IsAvailable
CreatedAt

5. BookCopies

BookCopyId
BookId
SerialNumber
Status (Available,Borrowed,Damaged,Lost)
IsAvailable
AddedDate

6. Borrowings

BorrowingId
MemberId
BookCopyId
BorrowDate
DueDate
ReturnDate
Status(Borrowed, Returned, Overdue)
CreatedAt

7. Fines

FineId
BorrowingId
FineAmount
IsPaid
CreatedDate


8. FinePayments

FinePaymentId
FineId
AmountPaid
PaymentDate
PaymentMethod



Relationships
MembershipType 1 -> Many Members

Category 1 -> Many Books

Book 1 -> Many BookCopies

Member 1 -> Many Borrowings

BookCopy 1 -> Many Borrowings

Borrowing 1 -> One Fine

Fine 1 -> Many FinePayments


Borrowing Rules

1. Inactive members cannot borrow books
2. Borrowing limit depends on membership type
3. Member cannot borrow same book twice simultaneously
4. Member cannot borrow if unpaid fine > ₹500
5. Only available book copies can be borrowed

Return Rules

1. Fine = ₹10 per delayed day
2. Returned copy becomes available
3. ReturnDate must be updated

