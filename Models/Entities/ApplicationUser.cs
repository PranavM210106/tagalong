using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Tagalong.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [MaxLength(200)]
        public string? ProfilePicture { get; set; }

        public bool IsVerified { get; set; } = false;

        // Navigation properties
        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public ICollection<Ride> OfferedRides { get; set; } = new List<Ride>();
        public ICollection<RideRequest> RideRequests { get; set; } = new List<RideRequest>();
        public ICollection<Booking> BookingsAsRider { get; set; } = new List<Booking>();
        public ICollection<Rating> RatingsGiven { get; set; } = new List<Rating>();
        public ICollection<Rating> RatingsReceived { get; set; } = new List<Rating>();
        public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
        public ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();

        // Computed properties
        public double? AverageRating => RatingsReceived?.Any() == true ? RatingsReceived.Average(r => (double)r.Stars) : (double?)null;
        public int RatingCount => RatingsReceived?.Count ?? 0;

        // Track when user was created
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
