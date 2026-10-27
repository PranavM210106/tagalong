using System.Collections.Generic;

namespace Tagalong.Models.ViewModels
{
    public class ChatThreadViewModel
    {
        public int BookingId { get; set; }
        public string OtherUserName { get; set; }
        public string OtherUserId { get; set; }
        public string CurrentUserId { get; set; }
        public int? RideId { get; set; }
        public List<ChatMessageViewModel> Messages { get; set; } = new();
    }

    public class ChatMessageViewModel
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
        public string SenderId { get; set; }
        public string Status { get; set; } = "Sent";
        public bool IsOwn { get; set; }
    }
}