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
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ProfileController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
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
        [Authorize]
        public IActionResult AddVehicle(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new Vehicle { VehicleType = "Car", SeatCapacity = 4 });
        }

        // POST: Profile/AddVehicle
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddVehicle(Vehicle vehicle, string returnUrl = null)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && !await _userManager.IsInRoleAsync(user, "Driver"))
            {
                await _userManager.AddToRoleAsync(user, "Driver");
                await _signInManager.RefreshSignInAsync(user);
            }

            vehicle.UserId = userId;
            ModelState.Remove(nameof(vehicle.UserId));
            ModelState.Remove(nameof(vehicle.Driver));
            ModelState.Remove(nameof(vehicle.Rides));

            if (ModelState.IsValid)
            {
                _context.Vehicles.Add(vehicle);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Vehicle registered successfully! You are now ready to offer rides.";

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(vehicle);
        }

        // POST: Profile/AddVehicleAjax (for inline modal in Offer Ride)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddVehicleAjax([FromBody] Vehicle vehicle)
        {
            if (vehicle == null)
            {
                return Json(new { success = false, message = "Invalid vehicle data" });
            }

            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && !await _userManager.IsInRoleAsync(user, "Driver"))
            {
                await _userManager.AddToRoleAsync(user, "Driver");
                await _signInManager.RefreshSignInAsync(user);
            }

            vehicle.UserId = userId;
            ModelState.Remove(nameof(vehicle.UserId));
            ModelState.Remove(nameof(vehicle.Driver));
            ModelState.Remove(nameof(vehicle.Rides));

            if (ModelState.IsValid)
            {
                _context.Vehicles.Add(vehicle);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    vehicle = new
                    {
                        vehicleId = vehicle.VehicleId,
                        model = vehicle.Model,
                        vehicleType = vehicle.VehicleType,
                        registrationNumber = vehicle.RegistrationNumber,
                        seatCapacity = vehicle.SeatCapacity
                    }
                });
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return Json(new { success = false, message = string.Join(", ", errors), errors });
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

        // POST: Profile/EditVehicle
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVehicle(Vehicle vehicle)
        {
            var userId = _userManager.GetUserId(User);
            var existing = await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == vehicle.VehicleId && v.UserId == userId);
            if (existing == null)
            {
                TempData["ErrorMessage"] = "Vehicle not found or unauthorized.";
                return RedirectToAction(nameof(Index));
            }

            existing.VehicleType = vehicle.VehicleType ?? "Car";
            existing.Model = vehicle.Model?.Trim() ?? existing.Model;
            existing.RegistrationNumber = vehicle.RegistrationNumber?.Trim().ToUpperInvariant() ?? existing.RegistrationNumber;
            existing.SeatCapacity = Math.Clamp(vehicle.SeatCapacity, 1, 8);

            _context.Update(existing);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Vehicle details updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Profile/DeleteVehicle
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVehicle(int VehicleId)
        {
            var userId = _userManager.GetUserId(User);
            var vehicle = await _context.Vehicles
                .Include(v => v.Rides)
                .FirstOrDefaultAsync(v => v.VehicleId == VehicleId && v.UserId == userId);

            if (vehicle == null)
            {
                TempData["ErrorMessage"] = "Vehicle not found or unauthorized.";
                return RedirectToAction(nameof(Index));
            }

            var hasActiveRides = vehicle.Rides != null && vehicle.Rides.Any(r => r.Status == "Active" && r.DepartureTime > DateTime.Now);
            if (hasActiveRides)
            {
                TempData["ErrorMessage"] = "Cannot delete vehicle with active scheduled rides. Cancel the rides first.";
                return RedirectToAction(nameof(Index));
            }

            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Vehicle removed successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Profile/AllRatings
        [Authorize]
        public async Task<IActionResult> AllRatings()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _context.Users.FindAsync(userId);
            var ratings = await _context.Ratings
                .Include(r => r.Rater)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Ride)
                .Where(r => r.RatedUserId == userId)
                .OrderByDescending(r => r.CreatedOn)
                .ToListAsync();

            ViewData["TargetUserName"] = user?.FullName ?? "Commuter";
            return View(ratings);
        }
    }
}
