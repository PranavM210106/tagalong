using System.Collections.Generic;
using Tagalong.Models.Entities;

namespace Tagalong.Models.ViewModels
{
    public class ProfileViewModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string ProfilePicture { get; set; }
        public bool IsVerified { get; set; }
        
        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        
        public double AverageRating { get; set; }
        public int RatingCount { get; set; }
        public ICollection<Rating> RecentRatings { get; set; } = new List<Rating>();
    }
}
