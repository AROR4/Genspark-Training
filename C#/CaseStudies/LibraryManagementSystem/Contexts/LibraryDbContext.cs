using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Models;
namespace LibraryManagementSystem;

public class LibraryDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=LibraryDb;Username=raghavarora;Password=yourpassword");
    }

    public DbSet<Book> Books { get; set; }

    public DbSet<BookCopy> BookCopies { get; set; }

    public DbSet<Borrowing> Borrowings { get; set; }

    public DbSet<Category> Categories { get; set; }

    public DbSet<Fine> Fines { get; set; }

    public DbSet<FinePayment> FinePayments { get; set; }

    public DbSet<Member> Members { get; set; }

    public DbSet<MembershipType> MembershipTypes { get; set; }

    public DbSet<User> Users{get;set;}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.BookId)
                .HasName("PK_BookId");

            entity.HasIndex(e => e.ISBN)
                .IsUnique();

            entity.Property(e => e.Title)
                .HasMaxLength(200);

            entity.Property(e => e.Author)
                .HasMaxLength(150);

            entity.Property(e => e.ISBN)
                .HasMaxLength(20);
            
            entity.Property(e => e.IsAvailable)
                .HasDefaultValue(true);
            
            entity.Property(e => e.Price)
            .HasPrecision(10, 2);

            entity.HasOne(d => d.Category)
                .WithMany(p => p.Books)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Books_Categories");
        });

        modelBuilder.Entity<BookCopy>(entity =>
        {
            entity.HasKey(e => e.BookCopyId)
                .HasName("PK_BookCopyId");

            entity.HasIndex(e => e.SerialNumber)
                .IsUnique();

            entity.Property(e => e.SerialNumber)
                .HasMaxLength(50);

            entity.Property(e => e.Status)
                .HasMaxLength(20);

            entity.Property(e => e.IsAvailable)
                .HasDefaultValue(true);

            entity.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "CK_BookCopies_Status",
                    "\"Status\" IN ('Available', 'Borrowed', 'Lost')"
                );
                 t.HasCheckConstraint(
                    "CK_BookCopies_DamagePercentage",
                    "\"DamagePercentage\" >= 0 AND \"DamagePercentage\" <= 100"
                );

            });


            entity.HasOne(d => d.Book)
                .WithMany(p => p.BookCopies)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_BookCopies_Books");
        });

        modelBuilder.Entity<Borrowing>(entity =>
        {
            entity.HasKey(e => e.BorrowingId)
                .HasName("PK_BorrowingId");

            entity.Property(e => e.BorrowDate)
                .HasColumnType("timestamp without time zone");

            entity.Property(e => e.DueDate)
                .HasColumnType("timestamp without time zone");

            entity.Property(e => e.ReturnDate)
                .HasColumnType("timestamp without time zone");

            entity.Property(e => e.Status)
                .HasMaxLength(20);

            entity.Property(e => e.FineAmount)
                .HasPrecision(10, 2);

            entity.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "CK_Borrowings_Status",
                    "\"Status\" IN ('Borrowed', 'Returned', 'Overdue')"
                );

                t.HasCheckConstraint(
                    "CK_Borrowings_OldDamagePercentage",
                    "\"OldDamagePercentage\" >= 0 AND \"OldDamagePercentage\" <= 100"
                );

                t.HasCheckConstraint(
                    "CK_Borrowings_NewDamagePercentage",
                    "\"NewDamagePercentage\" IS NULL OR (\"NewDamagePercentage\" >= 0 AND \"NewDamagePercentage\" <= 100)"
                );
            });

            entity.HasOne(d => d.BookCopy)
                .WithMany(p => p.Borrowings)
                .HasForeignKey(d => d.BookCopyId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Borrowings_BookCopies");

            entity.HasOne(d => d.Member)
                .WithMany(p => p.Borrowings)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Borrowings_Members");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId)
                .HasName("PK_CategoryId");

            entity.HasIndex(e => e.CategoryName)
                .IsUnique();

            entity.Property(e => e.CategoryName)
                .HasMaxLength(100);
        });

        modelBuilder.Entity<Fine>(entity =>
        {
            entity.HasKey(e => e.FineId)
                .HasName("PK_FineId");

            entity.Property(e => e.FineAmount)
                .HasPrecision(10, 2);

            entity.HasOne(d => d.Borrowing)
                .WithOne(p => p.Fine)
                .HasForeignKey<Fine>(d => d.BorrowingId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Fines_Borrowings");
        });

        modelBuilder.Entity<FinePayment>(entity =>
        {
            entity.HasKey(e => e.FinePaymentId)
                .HasName("PK_FinePaymentId");

            entity.Property(e => e.AmountPaid)
                .HasPrecision(10, 2);

            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50);

            entity.Property(e => e.PaymentDate)
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.Fine)
                .WithMany(p => p.FinePayments)
                .HasForeignKey(d => d.FineId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_FinePayments_Fines");
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(e => e.MemberId)
                .HasName("PK_MemberId");

            entity.HasIndex(e => e.Email)
                .IsUnique();

            entity.HasIndex(e => e.PhoneNumber)
                .IsUnique();

            entity.Property(e => e.FullName)
                .HasMaxLength(100);

            entity.Property(e => e.Email)
                .HasMaxLength(100);

            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15);

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.HasOne(d => d.MembershipType)
                .WithMany(p => p.Members)
                .HasForeignKey(d => d.MembershipTypeId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Members_MembershipTypes");
        });

        modelBuilder.Entity<MembershipType>(entity =>
        {
            entity.HasKey(e => e.MembershipTypeId)
                .HasName("PK_MembershipTypeId");

            entity.HasIndex(e => e.Name)
                .IsUnique();

            entity.Property(e => e.Name)
                .HasMaxLength(50);

            entity.HasData(
                new MembershipType
                {
                    MembershipTypeId = 1,
                    Name = "Basic",
                    MaxBorrowLimit = 2,
                    MaxBorrowDays = 7
                },
                new MembershipType
                {
                    MembershipTypeId = 2,
                    Name = "Student",
                    MaxBorrowLimit = 3,
                    MaxBorrowDays = 10
                },
                new MembershipType
                {
                    MembershipTypeId = 3,
                    Name = "Premium",
                    MaxBorrowLimit = 5,
                    MaxBorrowDays = 15
                }
            );
        }
        );
        modelBuilder.Entity<User>()
        .HasOne(u => u.Member)
        .WithOne(m => m.User)
        .HasForeignKey<User>(u => u.MemberId)
        .OnDelete(DeleteBehavior.Restrict);
    }
}
