using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tagalong.Models.Entities
{
    public class Complaint
    {
        [Key]
        public int ComplaintId { get; set; }

        [Required]
        public string UserId { get; set; }

        public int? BookingId { get; set; } // nullable as per spec

        [Required]
        [MaxLength(200)]
        public string Subject { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Open"; // Open/Resolved

        [Required]
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        [ForeignKey("BookingId")]
        public Booking Booking { get; set; }
    }
}
