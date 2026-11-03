using System;
using System.Collections.Generic;

namespace Tagalong.Models.ViewModels
{
    public class ChatMessageViewModel
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string Status { get; set; } = "Delivered";
        public bool IsOwn { get; set; }
    }

    public class ChatThreadViewModel
    {
        public int BookingId { get; set; }
        public string OtherUserName { get; set; } = string.Empty;
        public string OtherUserId { get; set; } = string.Empty;
        public string CurrentUserId { get; set; } = string.Empty;
        public int RideId { get; set; }
        public List<ChatMessageViewModel> Messages { get; set; } = new List<ChatMessageViewModel>();
    }
}
