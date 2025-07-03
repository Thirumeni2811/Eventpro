using Eventpro.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Eventpro.DataAccess
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        { }

        public DbSet<Users> Users { get; set; }
        public DbSet<Gallery> Gallery { get; set; }
        public DbSet<Services> Services { get; set; }
        public DbSet<Provides> Provides { get; set; }
        public DbSet<Tickets> Tickets { get; set; }
        public DbSet<Events> Events { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Users table config
            modelBuilder.Entity<Users>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Email).IsRequired();
                entity.Property(e => e.PhoneNo).IsRequired();
                entity.Property(e => e.Role).IsRequired();
            });

            // Services table config
            modelBuilder.Entity<Services>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title).IsRequired();
                entity.Property(e => e.Description).IsRequired();
            });

            // Provides table config
            modelBuilder.Entity<Provides>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title).IsRequired();
                entity.Property(e => e.Description).IsRequired();
            });

            // Gallery table config
            modelBuilder.Entity<Gallery>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Type).IsRequired();
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Description).IsRequired();
            });

            // Tickets table configuration
            modelBuilder.Entity<Tickets>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.EventId)
                    .IsRequired();

                entity.Property(e => e.UserId)
                    .IsRequired();

                entity.Property(e => e.Quantity)
                    .IsRequired();

                entity.Property(e => e.Type)
                    .IsRequired();

                entity.Property(e => e.TotalPrice)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.TicketPrice)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.BookingFee)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.PurchaseDate)
                    .IsRequired();

                entity.Property(e => e.PaymentStatus)
                    .HasMaxLength(50);

                // Relationships
                entity.HasOne(e => e.Event)
                    .WithMany()
                    .HasForeignKey(e => e.EventId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Events table config
            modelBuilder.Entity<Events>(entity =>
            {
                // Relationships
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

        }
    }
}
