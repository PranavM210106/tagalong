using System;
using System.ComponentModel.DataAnnotations;

namespace Tagalong.Models.ViewModels
{
    public class FindRideViewModel
    {
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
        [DataType(DataType.DateTime)]
        public DateTime PreferredTime { get; set; }
    }
}
