using System;
using System.ComponentModel.DataAnnotations;
using Tagalong.Models.Entities;

namespace Tagalong.Models.ViewModels
{
    public class OfferRideViewModel
    {
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
        [DataType(DataType.DateTime)]
        public DateTime DepartureTime { get; set; }

        [Required]
        [Range(1, 8)]
        public int SeatsAvailable { get; set; }

        [Required]
        [Range(0, 1000)]
        public decimal PricePerSeat { get; set; }

        [Required]
        public int VehicleId { get; set; }

        // For dropdown in the view
        public IEnumerable<Vehicle> Vehicles { get; set; }
    }
}
