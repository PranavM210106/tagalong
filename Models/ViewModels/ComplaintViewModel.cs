using System;
using System.ComponentModel.DataAnnotations;

namespace Tagalong.Models.ViewModels
{
    public class FileComplaintViewModel
    {
        public int? BookingId { get; set; }

        [Required]
        [MaxLength(200)]
        [Display(Name = "Subject")]
        public string Subject { get; set; }

        [Required]
        [MaxLength(2000)]
        [Display(Name = "Description")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
    }

    public class ComplaintDetailViewModel
    {
        public int ComplaintId { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public string UserName { get; set; }
        public int? BookingId { get; set; }
        public string RideInfo { get; set; }
    }
}