using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tagalong.Data;
using Tagalong.Models.Entities;

namespace Tagalong.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Home/Index (Dashboard)
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Get nearby active rides (simplified - in a real app you'd use user's saved location)
            var nearbyRides = await _context.Rides
                .Where(r => r.Status == "Active" && r.DriverId != user.Id)
                .Include(r => r.Driver)
                .Include(r => r.Vehicle)
                .Take(10) // Limit for demo
                .ToListAsync();

            return View(nearbyRides);
        }
    }
}
