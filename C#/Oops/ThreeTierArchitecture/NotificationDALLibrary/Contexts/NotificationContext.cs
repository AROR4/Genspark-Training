using Microsoft.EntityFrameworkCore;
using NotificationModelLibrary;

namespace NotificationDALLibrary.Contexts
{
    public class NotificationContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(
                "Host=localhost;Port=5432;Database=notificationdb;Username=postgres;Password=khdmdcm");
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Notification> Notifications { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(u =>
            {
                u.HasKey(u => u.Id).HasName("PK_UserId");
            });

            modelBuilder.Entity<Notification>(n =>
            {
                n.HasKey(n => n.Id).HasName("PK_NotificationId");
                n.HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(a => a.UserId)
                .HasConstraintName("FK_Notification_User")
                .OnDelete(DeleteBehavior.SetNull);
                // it is treated as logs so behavious is null if user deleted still logs should be there with recipient


                n.Property(n=>n.SentDateTime).HasColumnType("timestamp without time zone");
                n.Property(n => n.NotificationType)
                .HasConversion<string>();
            });
        }
    }
}
