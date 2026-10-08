using System.Collections.Generic;
using Tagalong.Models.Entities;

namespace Tagalong.Models.ViewModels
{
    public class MyRidesViewModel
    {
        public IEnumerable<Ride> AsDriver { get; set; } = new List<Ride>();
        public IEnumerable<Booking> AsRider { get; set; } = new List<Booking>();
    }
}
