using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Tagalong.Data;
using Tagalong.Models.Entities;
using Tagalong.Models.ViewModels;
using Tagalong.Services;

namespace Tagalong.Controllers
{
    public class RidesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDistanceService _distanceService;

        public RidesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IDistanceService distanceService)
        {
            _context = context;
            _userManager = userManager;
            _distanceService = distanceService;
        }

        // GET: Rides/Offer
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> Offer()
        {
            var userId = _userManager.GetUserId(User);
            var vehicles = await _context.Vehicles
                .Where(v => v.UserId == userId)
                .ToListAsync();

            var model = new OfferRideViewModel
            {
                Vehicles = vehicles.Select(v => new SelectListItem
                {
                    Value = v.VehicleId.ToString(),
                    Text = $"{v.Model} ({v.RegistrationNumber})"
                }).ToList()
            };

            return View(model);
        }

        // POST: Rides/Offer
        [HttpPost]
        [Authorize(Roles = "Driver")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Offer(OfferRideViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);

                var ride = new Ride
                {
                    DriverId = userId,
                    VehicleId = model.VehicleId,
                    StartLocation = model.StartLocation,
                    StartLat = model.StartLat,
                    StartLng = model.StartLng,
                    EndLocation = model.EndLocation,
                    EndLat = model.EndLat,
                    EndLng = model.EndLng,
                    DepartureTime = model.DepartureTime,
                    SeatsAvailable = model.SeatsAvailable,
                    PricePerSeat = model.PricePerSeat,
                    Status = "Active"
                };

                _context.Rides.Add(ride);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }

            // If we got this far, something failed, redisplay form
            var userId2 = _userManager.GetUserId(User);
            model.Vehicles = await _context.Vehicles
                .Where(v => v.UserId == userId2)
                .Select(v => new SelectListItem
                {
                    Value = v.VehicleId.ToString(),
                    Text = $"{v.Model} ({v.RegistrationNumber})"
                })
                .ToListAsync();

            return View(model);
        }

        // GET: Rides/Find
        [Authorize]
        public IActionResult Find()
        {
            return View();
        }

        // POST: Rides/Find
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Find(FindRideViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Get all active rides
                var rides = await _context.Rides
                    .Where(r => r.Status == "Active")
                    .Include(r => r.Driver)
                    .Include(r => r.Vehicle)
                    .ToListAsync();

                // Find matching rides using Haversine distance calculation
                var matchedRides = _distanceService.FindMatchingRides(model, rides)
                    .ToList();

                return View("Results", matchedRides);
            }

            return View(model);
        }

        // GET: Rides/Results
        [Authorize]
        public IActionResult Results(List<RideResultViewModel> matchedRides)
        {
            return View(matchedRides ?? new List<RideResultViewModel>());
        }

        // POST: Rides/RequestToJoin
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestToJoin(int rideId)
        {
            var userId = _userManager.GetUserId(User);
            
            // Check if the ride exists and is active
            var ride = await _context.Rides
                .Include(r => r.Driver)
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.RideId == rideId && r.Status == "Active");
                
            if (ride == null)
            {
                return NotFound();
            }

            // Check if user already has a booking for this ride
            var existingBooking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.RideId == rideId && b.RiderId == userId);
                
            if (existingBooking != null)
            {
                // Already booked, redirect to my rides
                return RedirectToAction("MyRides", "Bookings");
            }

            // Check if there are enough seats available
            var totalBookedSeats = await _context.Bookings
                .Where(b => b.RideId == rideId)
                .SumAsync(b => (int?)b.SeatsBooked) ?? 0;

            if (totalBookedSeats >= ride.SeatsAvailable)
            {
                ModelState.AddModelError(string.Empty, "Sorry, this ride is fully booked.");
                return RedirectToAction("Results", new { matchedRides = new List<RideResultViewModel> {
                    new RideResultViewModel { Ride = ride, Distance = 0, MatchQuality = "Exact Match" }
                }});
            }

            // Create booking
            var booking = new Booking
            {
                RideId = rideId,
                RiderId = userId,
                SeatsBooked = 1, // Default to 1 seat for simplicity
                BookingStatus = "Pending",
                BookedOn = DateTime.Now
            };

            _context.Bookings.Add(booking);
            
            // Update seats available
            ride.SeatsAvailable -= booking.SeatsBooked;
            
            await _context.SaveChangesAsync();
            return RedirectToAction("MyRides", "Bookings");
        }
    }
}
