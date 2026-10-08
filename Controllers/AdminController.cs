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
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Admin/ManageUsers
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _context.Users
                .OrderBy(u => u.FullName)
                .ToListAsync();
            return View(users);
        }

        // POST: Admin/ToggleBlockUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleBlockUser(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // In a real app, you might have a LockoutEnd property or similar
            // For simplicity, we'll just toggle a custom property or use the built-in lockout
            // Using the built-in ASP.NET Core Identity lockout feature
            if (user.LockoutEnd == null || user.LockoutEnd <= DateTime.Now)
            {
                // Lock the user out for 1 year (effectively blocking)
                await _userManager.SetLockoutEndDateAsync(user, DateTime.Now.AddYears(1));
            }
            else
            {
                // Unlock the user
                await _userManager.SetLockoutEndDateAsync(user, null);
            }

            return RedirectToAction(nameof(ManageUsers));
        }

        // POST: Admin/VerifyDriver
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyDriver(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Toggle the IsVerified flag
            user.IsVerified = !user.IsVerified;
            _context.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ManageUsers));
        }

        // GET: Admin/ManageRides
        public async Task<IActionResult> ManageRides()
        {
            var rides = await _context.Rides
                .Include(r => r.Driver)
                .Include(r => r.Vehicle)
                .OrderByDescending(r => r.DepartureTime)
                .ToListAsync();
            return View(rides);
        }

        // POST: Admin/ForceCancelRide
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForceCancelRide(int rideId)
        {
            var ride = await _context.Rides
                .Include(r => r.Bookings)
                .FirstOrDefaultAsync(r => r.RideId == rideId);

            if (ride == null)
            {
                return NotFound();
            }

            // Cancel the ride and all associated bookings
            ride.Status = "Cancelled";
            
            foreach (var booking in ride.Bookings)
            {
                if (booking.BookingStatus == "Pending" || booking.BookingStatus == "Confirmed")
                {
                    booking.BookingStatus = "Cancelled";
                    // Return the seats to the ride (though it's cancelled, just for consistency)
                    ride.SeatsAvailable += booking.SeatsBooked;
                }
            }

            _context.Update(ride);
            _context.UpdateRange(ride.Bookings);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ManageRides));
        }

        // GET: Admin/ViewComplaints
        public async Task<IActionResult> ViewComplaints()
        {
            var complaints = await _context.Complaints
                .Include(c => c.User)
                .Include(c => c.Booking)
                    .ThenInclude(b => b.Ride)
                .OrderByDescending(c => c.CreatedOn)
                .ToListAsync();
            return View(complaints);
        }

        // POST: Admin/ResolveComplaint
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResolveComplaint(int complaintId)
        {
            var complaint = await _context.Complaints.FindAsync(complaintId);
            if (complaint == null)
            {
                return NotFound();
            }

            complaint.Status = "Resolved";
            _context.Update(complaint);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ViewComplaints));
        }

        // GET: Admin/Analytics
        public async Task<IActionResult> Analytics()
        {
            var totalRides = await _context.Rides.CountAsync();
            var completedRides = await _context.Rides.CountAsync(r => r.Status == "Completed");
            var cancelledRides = await _context.Rides.CountAsync(r => r.Status == "Cancelled");
            var activeRides = await _context.Rides.CountAsync(r => r.Status == "Active");

            var totalUsers = await _context.Users.CountAsync();
            var totalDrivers = await _context.Users.CountAsync(u => u.OfferedRides.Any());
            var totalRiders = await _context.Users.CountAsync(u => u.BookingsAsRider.Any());

            // Most common routes (group by StartLocation/EndLocation)
            var topRoutes = await _context.Rides
                .Where(r => r.Status == "Completed")
                .GroupBy(r => new { r.StartLocation, r.EndLocation })
                .Select(g => new
                {
                    StartLocation = g.Key.StartLocation,
                    EndLocation = g.Key.EndLocation,
                    Count = g.Count()
                })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToListAsync();

            // Complaint stats
            var openComplaints = await _context.Complaints.CountAsync(c => c.Status == "Open");
            var resolvedComplaints = await _context.Complaints.CountAsync(c => c.Status == "Resolved");

            var model = new AdminAnalyticsViewModel
            {
                TotalRides = totalRides,
                CompletedRides = completedRides,
                CancelledRides = cancelledRides,
                ActiveRides = activeRides,
                TotalUsers = totalUsers,
                TotalDrivers = totalDrivers,
                TotalRiders = totalRiders,
                TopRoutes = topRoutes,
                OpenComplaints = openComplaints,
                ResolvedComplaints = resolvedComplaints
            };

            return View(model);
        }
    }
}
