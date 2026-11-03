using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Tagalong.Models.Entities
{
    public class Vehicle
    {
        [Key]
        public int VehicleId { get; set; }

        [ValidateNever]
        public string? UserId { get; set; }

        [Required(ErrorMessage = "Please select a vehicle type")]
        [MaxLength(50)]
        public string VehicleType { get; set; }

        [Required(ErrorMessage = "Registration number is required")]
        [MaxLength(20)]
        public string RegistrationNumber { get; set; }

        [Required(ErrorMessage = "Seat capacity is required")]
        [Range(1, 8, ErrorMessage = "Seat capacity must be between 1 and 8")]
        public int SeatCapacity { get; set; }

        [Required(ErrorMessage = "Vehicle model is required")]
        [MaxLength(50)]
        public string Model { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("UserId")]
        [ValidateNever]
        public ApplicationUser? Driver { get; set; }

        [ValidateNever]
        public ICollection<Ride> Rides { get; set; } = new List<Ride>();
    }
}
