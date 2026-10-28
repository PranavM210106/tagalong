using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Tagalong.Models.Entities;

namespace Tagalong.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Ride> Rides { get; set; }
        public DbSet<RideRequest> RideRequests { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Configure ApplicationUser properties
            builder.Entity<ApplicationUser>()
                .Property(u => u.FullName)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Entity<ApplicationUser>()
                .Property(u => u.PhoneNumber)
                .HasMaxLength(15);
                
            builder.Entity<ApplicationUser>()
                .Property(u => u.ProfilePicture)
                .HasMaxLength(200)
                .IsRequired(false);
                
            builder.Entity<ApplicationUser>()
                .Property(u => u.IsVerified)
                .HasDefaultValue(false);

            // Configure Vehicle entity
            builder.Entity<Vehicle>()
                .HasOne(v => v.Driver)
                .WithMany(u => u.Vehicles)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Ride entity
            builder.Entity<Ride>()
                .HasOne(r => r.Driver)
                .WithMany(u => u.OfferedRides)
                .HasForeignKey(r => r.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Ride>()
                .HasOne(r => r.Vehicle)
                .WithMany(v => v.Rides)
                .HasForeignKey(r => r.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure RideRequest entity
            builder.Entity<RideRequest>()
                .HasOne(rr => rr.Rider)
                .WithMany(u => u.RideRequests)
                .HasForeignKey(rr => rr.RiderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Booking entity
            builder.Entity<Booking>()
                .HasOne(b => b.Ride)
                .WithMany(r => r.Bookings)
                .HasForeignKey(b => b.RideId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.Entity<Booking>()
                .HasOne(b => b.Rider)
                .WithMany(u => u.BookingsAsRider)
                .HasForeignKey(b => b.RiderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Rating entity
            builder.Entity<Rating>()
                .HasOne(r => r.Booking)
                .WithOne(b => b.Rating)
                .HasForeignKey<Rating>(r => r.BookingId);

            builder.Entity<Rating>()
                .Property(r => r.Stars)
                .HasColumnType("decimal(2,1)");

            builder.Entity<Ride>()
                .Property(r => r.PricePerSeat)
                .HasColumnType("decimal(18,2)");

            // Configure ApplicationUser relationships for ratings
            builder.Entity<ApplicationUser>()
                .HasMany(u => u.RatingsGiven)
                .WithOne(r => r.Rater)
                .HasForeignKey(r => r.RatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ApplicationUser>()
                .HasMany(u => u.RatingsReceived)
                .WithOne(r => r.RatedUser)
                .HasForeignKey(r => r.RatedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Complaint entity
            builder.Entity<Complaint>()
                .HasOne(c => c.User)
                .WithMany(u => u.Complaints)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.Entity<Complaint>()
                .HasOne(c => c.Booking)
                .WithMany(b => b.Complaints)
                .HasForeignKey(c => c.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Message entity
            builder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.Entity<Message>()
                .Property(m => m.Timestamp)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }
}
