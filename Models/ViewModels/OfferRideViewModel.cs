using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Tagalong.Models.Entities;

namespace Tagalong.Models.ViewModels
{
    public class OfferRideViewModel
    {
        [Required(ErrorMessage = "Departure location is required")]
        [MaxLength(200)]
        public string StartLocation { get; set; }

        public double StartLat { get; set; }

        public double StartLng { get; set; }

        [Required(ErrorMessage = "Destination is required")]
        [MaxLength(200)]
        public string EndLocation { get; set; }

        public double EndLat { get; set; }

        public double EndLng { get; set; }

        [Required(ErrorMessage = "Departure date & time is required")]
        [DataType(DataType.DateTime)]
        public DateTime DepartureTime { get; set; }

        [Required(ErrorMessage = "Available seats is required")]
        [Range(1, 8, ErrorMessage = "Seats must be between 1 and 8")]
        public int SeatsAvailable { get; set; }

        [Required(ErrorMessage = "Price per seat is required")]
        [Range(0, 10000, ErrorMessage = "Price must be positive")]
        public decimal PricePerSeat { get; set; }

        [Required(ErrorMessage = "Please select a vehicle")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select or add a vehicle")]
        public int VehicleId { get; set; }

        // For display in the view only - excluded from model validation
        [ValidateNever]
        public IEnumerable<Vehicle>? Vehicles { get; set; }
    }
}
