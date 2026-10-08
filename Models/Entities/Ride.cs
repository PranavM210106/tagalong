using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tagalong.Models.Entities
{
    public class Ride
    {
        [Key]
        public int RideId { get; set; }

        [Required]
        public string DriverId { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [Required]
        [MaxLength(200)]
        public string StartLocation { get; set; }

        [Required]
        public double StartLat { get; set; }

        [Required]
        public double StartLng { get; set; }

        [Required]
        [MaxLength(200)]
        public string EndLocation { get; set; }

        [Required]
        public double EndLat { get; set; }

        [Required]
        public double EndLng { get; set; }

        [Required]
        public DateTime DepartureTime { get; set; }

        [Required]
        [Range(1, 8)]
        public int SeatsAvailable { get; set; }

        [Required]
        [Range(0, 1000)]
        public decimal PricePerSeat { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Active"; // Active/Completed/Cancelled

        // Navigation properties
        [ForeignKey("DriverId")]
        public ApplicationUser Driver { get; set; }

        [ForeignKey("VehicleId")]
        public Vehicle Vehicle { get; set; }

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
