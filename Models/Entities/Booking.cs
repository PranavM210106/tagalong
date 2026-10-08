using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tagalong.Models.Entities
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [Required]
        public int RideId { get; set; }

        [Required]
        public string RiderId { get; set; }

        [Required]
        [Range(1, 8)]
        public int SeatsBooked { get; set; }

        [Required]
        [MaxLength(20)]
        public string BookingStatus { get; set; } = "Pending"; // Pending/Confirmed/Cancelled

        [Required]
        public DateTime BookedOn { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("RideId")]
        public Ride Ride { get; set; }

        [ForeignKey("RiderId")]
        public ApplicationUser Rider { get; set; }

        public Rating Rating { get; set; }

        public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
    }
}
