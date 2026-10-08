using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tagalong.Models.Entities
{
    public class RideRequest
    {
        [Key]
        public int RequestId { get; set; }

        [Required]
        public string RiderId { get; set; }

        [Required]
        [MaxLength(200)]
        public string PickupLocation { get; set; }

        [Required]
        public double PickupLat { get; set; }

        [Required]
        public double PickupLng { get; set; }

        [Required]
        [MaxLength(200)]
        public string DropLocation { get; set; }

        [Required]
        public double DropLat { get; set; }

        [Required]
        public double DropLng { get; set; }

        [Required]
        public DateTime PreferredTime { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending/Matched/Expired/Cancelled

        // Navigation property
        [ForeignKey("RiderId")]
        public ApplicationUser Rider { get; set; }
    }
}
