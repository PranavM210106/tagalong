using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tagalong.Data;
using Tagalong.Models.Entities;
using Tagalong.Models.ViewModels;

namespace Tagalong.Controllers
{
    [Authorize]
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookingsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Bookings/MyRides
        public async Task<IActionResult> MyRides()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _context.Users
                .Include(u => u.OfferedRides)
                    .ThenInclude(r => r.Vehicle)
                .Include(u => u.BookingsAsRider)
                    .ThenInclude(b => b.Ride)
                        .ThenInclude(r => r.Driver)
                .Include(u => u.BookingsAsRider)
                    .ThenInclude(b => b.Ride)
                        .ThenInclude(r => r.Vehicle)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            var model = new MyRidesViewModel
            {
                AsDriver = user.OfferedRides.Where(r => r.Status == "Active" || r.Status == "Completed"),
                AsRider = user.BookingsAsRider.Where(b => b.BookingStatus == "Pending" || b.BookingStatus == "Confirmed")
                    .Select(b => b)
            };

            return View(model);
        }

        // POST: Bookings/ConfirmRider/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmRider(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Ride)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
            {
                return NotFound();
            }

            // Only the driver of the ride can confirm a rider
            var userId = _userManager.GetUserId(User);
            if (booking.Ride.DriverId != userId)
            {
                return Forbid();
            }

            booking.BookingStatus = "Confirmed";
            _context.Update(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(MyRides));
        }

        // POST: Bookings/CancelBooking/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Ride)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
            {
                return NotFound();
            }

            // Only the rider who made the booking or the driver of the ride can cancel
            var userId = _userManager.GetUserId(User);
            if (booking.RiderId != userId && booking.Ride.DriverId != userId)
            {
                return Forbid();
            }

            // If the rider cancels, we just update the booking status
            // If the driver cancels, we might want to cancel the entire ride? 
            // For simplicity, we'll just cancel the booking and adjust seats
            if (booking.RiderId == userId)
            {
                booking.BookingStatus = "Cancelled";
                // Return the seat to the ride
                booking.Ride.SeatsAvailable += booking.SeatsBooked;
            }
            else // Driver cancels the booking (but not the entire ride)
            {
                booking.BookingStatus = "Cancelled";
                booking.Ride.SeatsAvailable += booking.SeatsBooked;
            }

            _context.Update(booking);
            _context.Update(booking.Ride);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(MyRides));
        }

        // POST: Bookings/MarkCompleted/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkCompleted(int rideId)
        {
            var ride = await _context.Rides.FindAsync(rideId);
            if (ride == null)
            {
                return NotFound();
            }

            // Only the driver can mark the ride as completed
            var userId = _userManager.GetUserId(User);
            if (ride.DriverId != userId)
            {
                return Forbid();
            }

            ride.Status = "Completed";
            _context.Update(ride);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(MyRides));
        }
    }
}
