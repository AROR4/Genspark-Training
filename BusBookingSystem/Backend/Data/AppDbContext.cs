using BusBookingApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace BusBookingApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<BusOperator> BusOperators => Set<BusOperator>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<OperatorOffice> OperatorOffices => Set<OperatorOffice>();
    public DbSet<BusRoute> Routes => Set<BusRoute>();
    public DbSet<Bus> Buses => Set<Bus>();
    public DbSet<BusSchedule> BusSchedules => Set<BusSchedule>();
    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<SeatAvailability> SeatAvailabilities => Set<SeatAvailability>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingPassenger> BookingPassengers => Set<BookingPassenger>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users", table =>
            {
                table.HasCheckConstraint("ck_users_role", "role IN ('USER','OPERATOR','ADMIN')");
            });

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(150).IsRequired();
            entity.Property(e => e.PhoneNumber).HasColumnName("phone_number").HasMaxLength(20);
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").HasColumnType("text").IsRequired();
            entity.Property(e => e.Role).HasColumnName("role").HasMaxLength(20);
            entity.Property(e => e.IsApproved).HasColumnName("is_approved").HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<BusOperator>(entity =>
        {
            entity.ToTable("bus_operators", table =>
            {
                table.HasCheckConstraint("ck_bus_operators_approval_status", "approval_status IN ('PENDING','APPROVED','REJECTED','DISABLED')");
            });

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CompanyName).HasColumnName("company_name").HasMaxLength(150);
            entity.Property(e => e.LegalName).HasColumnName("legal_name").HasMaxLength(150);
            entity.Property(e => e.OwnerName).HasColumnName("owner_name").HasMaxLength(100);
            entity.Property(e => e.ContactEmail).HasColumnName("contact_email").HasMaxLength(150);
            entity.Property(e => e.ContactPhone).HasColumnName("contact_phone").HasMaxLength(20);
            entity.Property(e => e.RegistrationNumber).HasColumnName("registration_number").HasMaxLength(100);
            entity.Property(e => e.TaxNumber).HasColumnName("tax_number").HasMaxLength(100);
            entity.Property(e => e.LicenseNumber).HasColumnName("license_number").HasMaxLength(100);
            entity.Property(e => e.ApprovalStatus).HasColumnName("approval_status").HasMaxLength(20).HasDefaultValue("PENDING");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.AdminNotes).HasColumnName("admin_notes").HasColumnType("text");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(e => e.UserId).IsUnique();

            entity.HasOne(e => e.User)
                .WithMany(e => e.BusOperators)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.ToTable("cities");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();

            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<OperatorOffice>(entity =>
        {
            entity.ToTable("operator_offices");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.OperatorId).HasColumnName("operator_id");
            entity.Property(e => e.CityId).HasColumnName("city_id");
            entity.Property(e => e.Address).HasColumnName("address").HasColumnType("text");

            entity.HasIndex(e => new { e.OperatorId, e.CityId }).IsUnique();

            entity.HasOne(e => e.Operator)
                .WithMany(e => e.OperatorOffices)
                .HasForeignKey(e => e.OperatorId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.City)
                .WithMany(e => e.OperatorOffices)
                .HasForeignKey(e => e.CityId);
        });

        modelBuilder.Entity<BusRoute>(entity =>
        {
            entity.ToTable("routes");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SourceCityId).HasColumnName("source_city_id");
            entity.Property(e => e.DestinationCityId).HasColumnName("destination_city_id");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.SourceCity)
                .WithMany(e => e.SourceRoutes)
                .HasForeignKey(e => e.SourceCityId);

            entity.HasOne(e => e.DestinationCity)
                .WithMany(e => e.DestinationRoutes)
                .HasForeignKey(e => e.DestinationCityId);
        });

        modelBuilder.Entity<Bus>(entity =>
        {
            entity.ToTable("buses");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.OperatorId).HasColumnName("operator_id");
            entity.Property(e => e.TotalSeats).HasColumnName("total_seats").IsRequired();
            entity.Property(e => e.RegistrationNumber).HasColumnName("registration_number").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Company).HasColumnName("company").HasMaxLength(150).IsRequired();
            entity.Property(e => e.Type).HasColumnName("type").HasMaxLength(50).HasConversion<string>().IsRequired();
            entity.Property(e => e.LayoutJson).HasColumnName("layout_json").HasColumnType("jsonb");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.Operator)
                .WithMany(e => e.Buses)
                .HasForeignKey(e => e.OperatorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BusSchedule>(entity =>
        {
            entity.ToTable("bus_schedules");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BusId).HasColumnName("bus_id");
            entity.Property(e => e.RouteId).HasColumnName("route_id");
            entity.Property(e => e.SourceOfficeId).HasColumnName("source_office_id");
            entity.Property(e => e.DestinationOfficeId).HasColumnName("destination_office_id");
            entity.Property(e => e.TravelDate).HasColumnName("travel_date").HasColumnType("date").IsRequired();
            entity.Property(e => e.DepartureTime).HasColumnName("departure_time").HasColumnType("time").IsRequired();
            entity.Property(e => e.DurationMinutes).HasColumnName("duration_minutes").IsRequired();
            entity.Property(e => e.ArrivalDate).HasColumnName("arrival_date").HasColumnType("date").IsRequired();
            entity.Property(e => e.ArrivalTime).HasColumnName("arrival_time").HasColumnType("time").IsRequired();
            entity.Property(e => e.BasePrice).HasColumnName("base_price").HasColumnType("numeric(10,2)").IsRequired();
            entity.Property(e => e.IsCancelled).HasColumnName("is_cancelled").HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(e => new { e.BusId, e.TravelDate }).HasDatabaseName("idx_schedule_bus_date");
            entity.HasIndex(e => e.SourceOfficeId).HasDatabaseName("idx_schedule_source_office");
            entity.HasIndex(e => e.DestinationOfficeId).HasDatabaseName("idx_schedule_destination_office");

            entity.HasOne(e => e.Bus)
                .WithMany(e => e.BusSchedules)
                .HasForeignKey(e => e.BusId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Route)
                .WithMany(e => e.BusSchedules)
                .HasForeignKey(e => e.RouteId);

            entity.HasOne(e => e.SourceOffice)
                .WithMany()
                .HasForeignKey(e => e.SourceOfficeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.DestinationOffice)
                .WithMany()
                .HasForeignKey(e => e.DestinationOfficeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SeatAvailability>(entity =>
        {
            entity.ToTable("seat_availabilities", table =>
            {
                table.HasCheckConstraint("ck_seat_availabilities_status", "status IN ('Available','Held','Booked')");
            });

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BusScheduleId).HasColumnName("bus_schedule_id").IsRequired();
            entity.Property(e => e.SeatId).HasColumnName("seat_id").IsRequired();
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("Available");
            entity.Property(e => e.HoldExpiry).HasColumnName("hold_expiry").HasColumnType("timestamp");

            entity.HasIndex(e => new { e.BusScheduleId, e.SeatId }).IsUnique();
            entity.HasIndex(e => e.BusScheduleId).HasDatabaseName("idx_seat_availability_schedule");

            entity.HasOne(e => e.BusSchedule)
                .WithMany(e => e.SeatAvailabilities)
                .HasForeignKey(e => e.BusScheduleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Seat)
                .WithMany(e => e.SeatAvailabilities)
                .HasForeignKey(e => e.SeatId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Seat>(entity =>
        {
            entity.ToTable("seats");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BusId).HasColumnName("bus_id");
            entity.Property(e => e.SeatNumber).HasColumnName("seat_number").HasMaxLength(10);

            entity.HasIndex(e => new { e.BusId, e.SeatNumber }).IsUnique();
            entity.HasIndex(e => e.BusId).HasDatabaseName("idx_seat_bus");

            entity.HasOne(e => e.Bus)
                .WithMany(e => e.Seats)
                .HasForeignKey(e => e.BusId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.ToTable("bookings", table =>
            {
                table.HasCheckConstraint("ck_bookings_status", "status IN ('PENDING','CONFIRMED','FAILED','CANCELLED','EXPIRED')");
            });

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(e => e.ScheduleId).HasColumnName("schedule_id").IsRequired();
            entity.Property(e => e.BaseAmount).HasColumnName("base_amount").HasColumnType("numeric(10,2)").HasDefaultValue(0m);
            entity.Property(e => e.GstAmount).HasColumnName("gst_amount").HasColumnType("numeric(10,2)").HasDefaultValue(0m);
            entity.Property(e => e.ConvenienceFee).HasColumnName("convenience_fee").HasColumnType("numeric(10,2)").HasDefaultValue(0m);
            entity.Property(e => e.TotalAmount).HasColumnName("total_amount").HasColumnType("numeric(10,2)").HasDefaultValue(0m);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("PENDING").IsRequired();
            entity.Property(e => e.ContactEmail).HasColumnName("contact_email").HasMaxLength(150);
            entity.Property(e => e.ContactPhone).HasColumnName("contact_phone").HasMaxLength(20);
            entity.Property(e => e.RefundAmount).HasColumnName("refund_amount").HasColumnType("numeric(10,2)");
            entity.Property(e => e.OperatorLoss).HasColumnName("operator_loss").HasColumnType("numeric(10,2)");
            entity.Property(e => e.AdminRevenue).HasColumnName("admin_revenue").HasColumnType("numeric(10,2)");
            entity.Property(e => e.CancelledAt).HasColumnName("cancelled_at").HasColumnType("timestamp");
            entity.Property(e => e.CancellationType).HasColumnName("cancellation_type").HasMaxLength(30);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(e => e.UserId).HasDatabaseName("idx_booking_user");
            entity.HasIndex(e => e.ScheduleId).HasDatabaseName("idx_booking_schedule");

            entity.HasOne(e => e.User)
                .WithMany(e => e.Bookings)
                .HasForeignKey(e => e.UserId);

            entity.HasOne(e => e.Schedule)
                .WithMany(e => e.Bookings)
                .HasForeignKey(e => e.ScheduleId);
        });

        modelBuilder.Entity<BookingPassenger>(entity =>
        {
            entity.ToTable("booking_passengers");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100);
            entity.Property(e => e.Age).HasColumnName("age");
            entity.Property(e => e.Gender).HasColumnName("gender").HasMaxLength(10);
            entity.Property(e => e.SeatAvailabilityId).HasColumnName("seat_availability_id");

            entity.HasOne(e => e.Booking)
                .WithMany(e => e.BookingPassengers)
                .HasForeignKey(e => e.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.SeatAvailability)
                .WithMany()
                .HasForeignKey(e => e.SeatAvailabilityId);
        });

        modelBuilder.Entity<User>().HasData(new User
        {
            Id = 1,
            Name = "Admin",
            Email = "admin@bus.com",
            PasswordHash = "AQAAAAIAAYagAAAAEHh3VDh4vhphjjdi+hxd+ftJk3d9f4La+cRv1Uvu499Lu5aXf+HhPsRS0eZ7HOxXUw==",
            Role = "ADMIN",
            IsApproved = true,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("payments", table =>
            {
                table.HasCheckConstraint("ck_payments_status", "status IN ('SUCCESS','FAILED','PENDING')");
                table.HasCheckConstraint("ck_payments_refund_status", "refund_status IN ('NONE','INITIATED','COMPLETED')");
            });

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.Amount).HasColumnName("amount").HasColumnType("numeric(10,2)");
            entity.Property(e => e.PaymentMethod).HasColumnName("payment_method").HasMaxLength(50);
            entity.Property(e => e.PaymentReference).HasColumnName("payment_reference").HasMaxLength(100);
            entity.Property(e => e.GatewayOrderId).HasColumnName("gateway_order_id").HasMaxLength(100);
            entity.Property(e => e.GatewayPaymentId).HasColumnName("gateway_payment_id").HasMaxLength(100);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20);
            entity.Property(e => e.FailureReason).HasColumnName("failure_reason").HasColumnType("text");
            entity.Property(e => e.RefundStatus).HasColumnName("refund_status").HasMaxLength(20);
            entity.Property(e => e.PaidAt).HasColumnName("paid_at").HasColumnType("timestamp");
            entity.Property(e => e.RefundedAt).HasColumnName("refunded_at").HasColumnType("timestamp");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.Booking)
                .WithMany(e => e.Payments)
                .HasForeignKey(e => e.BookingId);
        });
        
    }
}
