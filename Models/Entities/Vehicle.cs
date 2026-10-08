using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tagalong.Models.Entities
{
    public class Vehicle
    {
        [Key]
        public int VehicleId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string VehicleType { get; set; }

        [Required]
        [MaxLength(20)]
        public string RegistrationNumber { get; set; }

        [Required]
        [Range(1, 8)]
        public int SeatCapacity { get; set; }

        [Required]
        [MaxLength(50)]
        public string Model { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public ApplicationUser Driver { get; set; }

        public ICollection<Ride> Rides { get; set; } = new List<Ride>();
    }
}
