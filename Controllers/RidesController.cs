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
        // Allow any authenticated user to access; we handle role-based logic inside
        [Authorize]
        public async Task<IActionResult> Offer()
        {
            // Check if user is a Driver
            if (!User.IsInRole("Driver"))
            {
                // If user is a Rider, redirect to BecomeDriver page
                if (User.IsInRole("Rider"))
                {
                    return RedirectToAction(nameof(BecomeDriver));
                }
                // For other roles (Admin, etc.), redirect to home
                return RedirectToAction("Index", "Home");
            }

            var userId = _userManager.GetUserId(User);
            var vehicles = await _context.Vehicles
                .Where(v => v.UserId == userId)
                .ToListAsync();

            var model = new OfferRideViewModel
            {
                Vehicles = vehicles
            };

            return View(model);
        }

        // GET: Rides/BecomeDriver - Friendly page for riders who want to become drivers
        [Authorize(Roles = "Rider")]
        public IActionResult BecomeDriver()
        {
            return View();
        }

        // POST: Rides/Offer
        [HttpPost]
        [Authorize(Roles = "Driver")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Offer(OfferRideViewModel model)
        {
            ModelState.Remove(nameof(model.Vehicles));

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
        public async Task<IActionResult> RequestToJoin(int rideId, int seatsBooked = 1)
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

            // Cannot book your own offered ride
            if (ride.DriverId == userId)
            {
                TempData["ErrorMessage"] = "You cannot book your own offered ride.";
                return RedirectToAction("MyRides", "Bookings");
            }

            // Check if user already has a booking for this ride
            var existingBooking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.RideId == rideId && b.RiderId == userId);
                
            if (existingBooking != null)
            {
                // Already booked, redirect to my rides
                return RedirectToAction("MyRides", "Bookings");
            }

            if (ride.SeatsAvailable <= 0)
            {
                TempData["ErrorMessage"] = "Sorry, this ride is already fully booked.";
                return RedirectToAction("Results", "Rides", new { pickup = ride.StartLocation, dropoff = ride.EndLocation });
            }

            int validSeats = Math.Clamp(seatsBooked, 1, ride.SeatsAvailable);

            // Create booking
            var booking = new Booking
            {
                RideId = rideId,
                RiderId = userId,
                SeatsBooked = validSeats,
                BookingStatus = "Pending",
                BookedOn = DateTime.Now
            };

            _context.Bookings.Add(booking);
            
            // Update seats available
            ride.SeatsAvailable -= validSeats;
            
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Ride requested successfully for {validSeats} seat(s)!";
            return RedirectToAction("MyRides", "Bookings");
        }
    }
}
