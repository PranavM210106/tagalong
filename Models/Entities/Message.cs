using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tagalong.Models.Entities
{
    public class Message
    {
        [Key]
        public int MessageId { get; set; }

        [Required]
        public string SenderId { get; set; }

        [Required]
        public string ReceiverId { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        // Foreign key to Booking
        public int BookingId { get; set; }

        // Navigation properties
        [ForeignKey("SenderId")]
        public ApplicationUser Sender { get; set; }

        [ForeignKey("ReceiverId")]
        public ApplicationUser Receiver { get; set; }

        [ForeignKey("BookingId")]
        public Booking Booking { get; set; }
    }
}
