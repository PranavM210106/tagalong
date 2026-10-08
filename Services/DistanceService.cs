using System;
using System.Collections.Generic;
using System.Linq;
using Tagalong.Models.Entities;
using Tagalong.Models.ViewModels;

namespace Tagalong.Services
{
    public class DistanceService : IDistanceService
    {
        // Haversine formula to calculate distance between two coordinates in kilometers
        public double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Radius of the earth in km
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = 
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Asin(Math.Sqrt(a));
            return R * c; // Distance in km
        }

        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        // Find rides that match the rider's request based on route proximity
        public IEnumerable<RideResultViewModel> FindMatchingRides(FindRideViewModel request, IEnumerable<Ride> rides)
        {
            const double distanceThresholdKm = 2.0; // 2 km threshold as mentioned in spec
            var results = new List<RideResultViewModel>();

            foreach (var ride in rides)
            {
                // Calculate distance from rider's pickup to ride's start point
                double pickupToStartDist = CalculateHaversineDistance(
                    request.PickupLat, request.PickupLng,
                    ride.StartLat, ride.StartLng);

                // Calculate distance from rider's drop to ride's end point
                double dropToEndDist = CalculateHaversineDistance(
                    request.DropLat, request.DropLng,
                    ride.EndLat, ride.EndLng);

                // If both points are within threshold, consider it a match
                if (pickupToStartDist <= distanceThresholdKm && dropToEndDist <= distanceThresholdKm)
                {
                    // Calculate average distance as a measure of match quality
                    double avgDistance = (pickupToStartDist + dropToEndDist) / 2;
                    
                    results.Add(new RideResultViewModel
                    {
                        Ride = ride,
                        Distance = avgDistance,
                        MatchQuality = GetMatchQuality(avgDistance)
                    });
                }
            }

            // Sort by match quality (best first) then by distance
            return results.OrderBy(r => r.MatchQuality).ThenBy(r => r.Distance);
        }

        private string GetMatchQuality(double distanceKm)
        {
            if (distanceKm <= 0.5) return "Excellent";
            if (distanceKm <= 1.0) return "Good";
            if (distanceKm <= 1.5) return "Fair";
            return "Poor";
        }
    }
}
