using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tagalong.Models.Entities
{
    public class Rating
    {
        [Key]
        public int RatingId { get; set; }

        [Required]
        public int BookingId { get; set; }

        [Required]
        public string RatedBy { get; set; } // UserId of who gave the rating

        [Required]
        public string RatedUserId { get; set; } // UserId of who received the rating

        [Required]
        [Range(1, 5)]
        public decimal Stars { get; set; }

        [MaxLength(500)]
        public string Comment { get; set; }

        // Navigation properties
        [ForeignKey("BookingId")]
        public Booking Booking { get; set; }

        [ForeignKey("RatedBy")]
        public ApplicationUser Rater { get; set; }

        [ForeignKey("RatedUserId")]
        public ApplicationUser RatedUser { get; set; }
    }
}
