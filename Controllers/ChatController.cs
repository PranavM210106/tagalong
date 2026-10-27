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
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Chat/Thread/5
        public async Task<IActionResult> Thread(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Ride)
                .ThenInclude(r => r.Driver)
                .Include(b => b.Rider)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
            {
                return NotFound();
            }

            // Check if the current user is either the rider or the driver of this booking
            var userId = _userManager.GetUserId(User);
            if (booking.RiderId != userId && booking.Ride.DriverId != userId)
            {
                return Forbid();
            }

            // Get messages for this booking
            var messages = await _context.Messages
                .Where(m => m.BookingId == bookingId)
                .OrderBy(m => m.Timestamp)
                .Select(m => new ChatMessageViewModel
                {
                    Id = m.MessageId,
                    Content = m.Content,
                    SentAt = m.Timestamp,
                    SenderId = m.SenderId,
                    Status = "Delivered",
                    IsOwn = m.SenderId == userId
                })
                .ToListAsync();

            var otherUserId = booking.RiderId == userId ? booking.Ride.DriverId : booking.RiderId;
            var otherUserName = booking.RiderId == userId
                ? booking.Ride.Driver.FullName
                : booking.Rider.FullName;

            var viewModel = new ChatThreadViewModel
            {
                BookingId = bookingId,
                OtherUserName = otherUserName,
                OtherUserId = otherUserId,
                CurrentUserId = userId,
                RideId = booking.RideId,
                Messages = messages
            };

            ViewData["CurrentUserId"] = userId;
            ViewData["OtherUserName"] = otherUserName;
            ViewData["OtherUserId"] = otherUserId;
            ViewData["RideId"] = booking.RideId;

            return View(viewModel);
        }

        // POST: Chat/SendMessage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(int bookingId, string messageText)
        {
            if (string.IsNullOrWhiteSpace(messageText))
            {
                return RedirectToAction(nameof(Thread), new { bookingId });
            }

            var booking = await _context.Bookings
                .Include(b => b.Ride)
                .Include(b => b.Rider)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
            {
                return NotFound();
            }

            // Check if the current user is either the rider or the driver of this booking
            var userId = _userManager.GetUserId(User);
            if (booking.RiderId != userId && booking.Ride.DriverId != userId)
            {
                return Forbid();
            }

            var message = new Message
            {
                BookingId = bookingId,
                SenderId = userId,
                ReceiverId = booking.RiderId == userId ? booking.Ride.DriverId : booking.RiderId,
                Content = messageText.Trim()
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Thread), new { bookingId });
        }
    }
}
