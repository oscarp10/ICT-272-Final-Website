using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourismWebsite.Data;
using TourismWebsite.Models;

namespace TourismWebsite.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public BookingsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Admin - View all bookings
        public async Task<IActionResult> Index()
        {
            // Include Tour info to display in table
            var bookings = await _context.TouristBookings
                .Include(b => b.Tour)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            return View("AdminBook", bookings); // Admin-only Razor page
        }

        // POST: Admin - Update booking status
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int bookingId, string status)
        {
            var booking = await _context.TouristBookings.FindAsync(bookingId);
            if (booking == null) return NotFound();

            booking.Status = status;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin - View booking details
        // Use 'id' to match the default route {controller}/{action}/{id}
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.TouristBookings // Ensure this matches your DbSet name
                .Include(b => b.Tour)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null) return NotFound();

            return View(booking);
        }


        // POST: Admin - Delete a booking
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var booking = await _context.TouristBookings.FindAsync(id);
            if (booking == null) return NotFound();

            _context.TouristBookings.Remove(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
