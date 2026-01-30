using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourismWebsite.Data;
using TourismWebsite.Models;

namespace TourismWebsite.Controllers
{
    [Authorize(Roles = "Tourist")]
    public class TouristBookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TouristBookingController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: TouristBooking/Create/5
        public async Task<IActionResult> Create(int tourId)
        {
            var tour = await _context.Tours.FindAsync(tourId);
            if (tour == null) return NotFound();

            var booking = new TouristBooking
            {
                TourId = tour.TourId,
                Tour = tour,
                NumPeople = 1 // default 1 person
            };

            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TouristBooking booking)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // REMOVE these from validation because we set them manually below
            ModelState.Remove("UserId");
            ModelState.Remove("Tour");
            ModelState.Remove("Status");

            var tour = await _context.Tours.FindAsync(booking.TourId);
            if (tour == null) return NotFound();

            if (ModelState.IsValid)
            {
                booking.UserId = user.Id; // Now we assign it safely
                booking.TotalPrice = tour.Price * booking.NumPeople;
                booking.Status = "Pending";
                booking.BookingDate = DateTime.Now;

                _context.TouristBookings.Add(booking);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // If validation fails, reload the tour info so the View doesn't crash
            booking.Tour = tour;
            return View(booking);
        }

        // GET: TouristBooking/Index
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var bookings = await _context.TouristBookings
                .Include(b => b.Tour)
                .Where(b => b.UserId == user.Id)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            return View(bookings);
        }

        // GET: TouristBooking/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);

            // Find the booking, include Tour details, and ensure it belongs to the current user
            var booking = await _context.TouristBookings
                .Include(b => b.Tour)
                .FirstOrDefaultAsync(m => m.BookingId == id && m.UserId == user.Id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // GET: TouristBooking/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);

            var booking = await _context.TouristBookings
                .Include(b => b.Tour)
                .FirstOrDefaultAsync(m => m.BookingId == id && m.UserId == user.Id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        // POST: TouristBooking/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var booking = await _context.TouristBookings
                .FirstOrDefaultAsync(m => m.BookingId == id && m.UserId == user.Id);

            if (booking != null)
            {
                _context.TouristBookings.Remove(booking);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }


}
