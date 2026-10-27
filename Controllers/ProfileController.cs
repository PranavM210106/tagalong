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
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Profile/Index
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _context.Users
                .Include(u => u.Vehicles)
                .Include(u => u.RatingsReceived)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            var model = new ProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProfilePicture = user.ProfilePicture,
                IsVerified = user.IsVerified,
                Vehicles = user.Vehicles.ToList(),
                AverageRating = user.RatingsReceived.Any()
                    ? (double)Math.Round(user.RatingsReceived.Average(r => r.Stars), 1)
                    : 0,
                RatingCount = user.RatingsReceived.Count(),
                RecentRatings = user.RatingsReceived
                    .OrderByDescending(r => r.Booking.BookedOn)
                    .Take(5)
                    .ToList()
            };

            return View(model);
        }

        // GET: Profile/Edit
        public async Task<IActionResult> Edit()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var model = new ProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProfilePicture = user.ProfilePicture
            };

            return View(model);
        }

        // POST: Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    return NotFound();
                }

                user.FullName = model.FullName;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                user.ProfilePicture = model.ProfilePicture;

                _context.Update(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: Profile/AddVehicle
        [Authorize(Roles = "Driver")]
        public IActionResult AddVehicle()
        {
            return View(new Vehicle());
        }

        // POST: Profile/AddVehicle
        [HttpPost]
        [Authorize(Roles = "Driver")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddVehicle(Vehicle model)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                model.UserId = userId;
                _context.Vehicles.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // POST: Profile/SubmitRating
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitRating(Rating model)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                model.RatedBy = userId;
                model.RatedUserId = model.RatedUserId; // This comes from the form
                _context.Ratings.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: Profile/FileComplaint
        public async Task<IActionResult> FileComplaint(int? bookingId)
        {
            var userId = _userManager.GetUserId(User);

            // If bookingId provided, verify it belongs to the user
            Booking booking = null;
            if (bookingId.HasValue)
            {
                booking = await _context.Bookings
                    .Include(b => b.Ride)
                        .ThenInclude(r => r.Driver)
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId.Value && b.RiderId == userId);

                if (booking == null)
                {
                    return NotFound();
                }
            }

            var model = new FileComplaintViewModel
            {
                BookingId = bookingId
            };

            if (booking != null)
            {
                ViewData["BookingInfo"] = $"{booking.Ride.StartLocation} → {booking.Ride.EndLocation} on {booking.Ride.DepartureTime:MMM dd, yyyy}";
            }

            return View(model);
        }

        // POST: Profile/FileComplaint
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FileComplaint(FileComplaintViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);

                var complaint = new Complaint
                {
                    UserId = userId,
                    BookingId = model.BookingId,
                    Subject = model.Subject,
                    Description = model.Description,
                    Status = "Open",
                    CreatedOn = DateTime.Now
                };

                _context.Complaints.Add(complaint);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Your complaint has been filed successfully. Our team will review it shortly.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: Profile/MyComplaints
        public async Task<IActionResult> MyComplaints()
        {
            var userId = _userManager.GetUserId(User);
            var complaints = await _context.Complaints
                .Include(c => c.Booking)
                    .ThenInclude(b => b.Ride)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedOn)
                .ToListAsync();

            return View(complaints);
        }
    }
}
