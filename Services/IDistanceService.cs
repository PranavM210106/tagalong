using System.Collections.Generic;
using Tagalong.Models.ViewModels;

namespace Tagalong.Services
{
    public interface IDistanceService
    {
        double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2);
        IEnumerable<RideResultViewModel> FindMatchingRides(FindRideViewModel request, IEnumerable<Tagalong.Models.Entities.Ride> rides);
    }
}
