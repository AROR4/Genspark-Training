# Community Library Membership & Book Lending System


# Technologies Used

- C#
- .NET Core Console Application
- Entity Framework Core
- PostgreSQL
- Npgsql

---

# Features Implemented

## Authentication
- Admin Login
- User Login

## Member Management
- Add member
- Search member
- Update membership type
- Deactivate member

## Book Management
- Add books
- Add book copies
- Search books
- View available books
- Mark damaged/lost books

## Borrowing Module
- Borrow books
- Membership-based borrowing limits
- Duplicate borrowing validation
- Fine limit validation
- Transaction management

## Return Module
- Return books
- Late fine calculation
- Damage fine calculation
- Lost book handling
- PostgreSQL function integration

## Fine Management
- View pending fines
- Pay fines
- Fine history

## Reports
- Available books by category
- Overdue books
- Most borrowed books
- Pending fines
- Member borrowing summary

---

# Project Structure

```text
LibraryManagementSystem/
│
├── Contexts/
├── Models/
├── Interfaces/
├── Repositories/
├── Services/
├── Exceptions/
├── Reports/
├── Migrations/
├── Program.cs
└── appsettings.json
```

---

# Files Included In Repository

- Requirement Understanding Document
- Database Design
- ER Diagram
- Console Application Source Code
- PostgreSQL Scripts
- EF Core Migration Files
- Test Cases
- Demo Screenshots / Output Logs

---

# Database Setup

## Step 1 — Create PostgreSQL Database

```sql
CREATE DATABASE LibraryManagementSystem;
```

---

## Step 2 — Update Connection String

Update connection string inside:

```text
LibraryDbContext.cs
```

Example:

```csharp
Host=localhost;
Port=5432;
Database=LibraryManagementSystem;
Username=postgres;
Password=yourpassword
```

---

# Run EF Core Migration

Open terminal inside project folder.

## Apply Migrations

```bash
dotnet ef database update
```

---

# PostgreSQL Functions

Execute PostgreSQL scripts manually:
- process_book_return()
- report functions

inside pgAdmin / PostgreSQL Query Tool.

---

# Seed Initial Admin Login

Run this SQL query:

```sql
INSERT INTO "Users"
(
    "Username",
    "Password",
    "Role"
)
VALUES
(
    'admin',
    'admin123',
    'Admin'
);
```

---

# Optional Seed Data

Add:
- categories
- books
- book copies
- membership types

using provided SQL scripts.

---

# Run Application

Open terminal inside project folder.

Run:

```bash
dotnet run
```

---

# Default Admin Credentials

```text
Username: admin
Password: admin123
```

---

# Important Business Rules

- Only active members can borrow books
- Borrow limits depend on membership type
- Duplicate borrowing not allowed
- Fine above ₹500 blocks borrowing
- Fully damaged books are marked lost
- Transactions ensure rollback on failure

---

# Test Cases Covered

- Borrowing limits
- Late return fine
- Damage fine calculation
- Transaction rollback
- PostgreSQL function execution
- Reports generation

---

# How to Generate Demo Data

1. Add categories
2. Add books
3. Add book copies
4. Add members
5. Add users linked with members
6. Perform borrow/return operations

---

# Author

Raghav Arora
