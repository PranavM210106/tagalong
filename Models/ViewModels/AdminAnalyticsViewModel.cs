using System.Collections.Generic;
using Tagalong.Models.Entities;

namespace Tagalong.Models.ViewModels
{
    public class AdminAnalyticsViewModel
    {
        public int TotalRides { get; set; }
        public int CompletedRides { get; set; }
        public int CancelledRides { get; set; }
        public int ActiveRides { get; set; }
        public int TotalUsers { get; set; }
        public int TotalDrivers { get; set; }
        public int TotalRiders { get; set; }
        
        // For top routes - using anonymous type from query, but for ViewModel we'll use a simple class
        public IEnumerable<dynamic> TopRoutes { get; set; } = new List<dynamic>();
        
        public int OpenComplaints { get; set; }
        public int ResolvedComplaints { get; set; }
    }
}
