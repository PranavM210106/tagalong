using Tagalong.Models.Entities;

namespace Tagalong.Models.ViewModels
{
    public class RideResultViewModel
    {
        public Ride Ride { get; set; }
        public double Distance { get; set; } // Distance in km
        public string MatchQuality { get; set; } // Excellent/Good/Fair/Poor
    }
}
