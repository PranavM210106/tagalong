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

        // For top routes
        public IEnumerable<dynamic> TopRoutes { get; set; } = new List<dynamic>();

        public int OpenComplaints { get; set; }
        public int ResolvedComplaints { get; set; }

        // Top drivers by category
        public IEnumerable<TopDriverViewModel> TopDriversByRating { get; set; } = new List<TopDriverViewModel>();
        public IEnumerable<TopDriverViewModel> TopDriversByRides { get; set; } = new List<TopDriverViewModel>();
        public IEnumerable<TopDriverViewModel> TopDriversByEarnings { get; set; } = new List<TopDriverViewModel>();

        // Chart data
        public List<ChartDataPoint> RidesTrendData { get; set; } = new List<ChartDataPoint>();
        public List<ChartDataPoint> UserGrowthData { get; set; } = new List<ChartDataPoint>();
        public List<ChartDataPoint> RevenueData { get; set; } = new List<ChartDataPoint>();
    }

    public class TopDriverViewModel
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public double AverageRating { get; set; }
        public int TotalRides { get; set; }
        public decimal TotalEarnings { get; set; }
        public int CompletedRides { get; set; }
        public string VehicleInfo { get; set; }
    }

    public class ChartDataPoint
    {
        public string Label { get; set; }
        public int Value { get; set; }
        public decimal DecimalValue { get; set; }
    }
}
