using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourismWebsite.Data;
using TourismWebsite.Models;

namespace TourismWebsite.Controllers
{
    [Authorize]
    public class FeedbacksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public FeedbacksController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Inside FeedbacksController.cs
        [AllowAnonymous]
        public async Task<IActionResult> Index(int? tourId)
        {
            var query = _context.Feedbacks
                .Include(f => f.Tour)
                .Include(f => f.User)
                .AsQueryable();

            // If a tourId is passed, show only that tour's feedback
            // If no tourId is passed, show EVERYTHING (Admin view)
            if (tourId.HasValue)
            {
                query = query.Where(f => f.TourId == tourId);
            }

            var results = await query.OrderByDescending(f => f.CreatedAt).ToListAsync();
            return View(results);
        }

        // --- CREATE ---
        [Authorize(Roles = "Tourist")]
        public IActionResult Create(int tourId)
        {
            var tour = _context.Tours.Find(tourId);
            if (tour == null) return NotFound();

            ViewBag.TourTitle = tour.Title;

            var model = new Feedback { TourId = tourId };
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Tourist")]
        public async Task<IActionResult> Create(Feedback feedback)
        {
            ModelState.Remove("UserId");
            ModelState.Remove("User");
            ModelState.Remove("Tour");

            if (!ModelState.IsValid)
            {
                var tour = await _context.Tours.FindAsync(feedback.TourId);
                ViewBag.TourTitle = tour?.Title;
                return View(feedback);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            feedback.UserId = currentUser.Id;
            feedback.CreatedAt = DateTime.Now;

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Tours", new { id = feedback.TourId });
        }

        // --- EDIT ---
        [Authorize(Roles = "Tourist")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var feedback = await _context.Feedbacks.FindAsync(id);
            var currentUser = await _userManager.GetUserAsync(User);

            // Security: Only the owner can edit
            if (feedback == null || feedback.UserId != currentUser.Id) return Unauthorized();

            return View(feedback);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Tourist")]
        public async Task<IActionResult> Edit(int id, Feedback feedback)
        {
            if (id != feedback.FeedbackId) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            var existingFeedback = await _context.Feedbacks.AsNoTracking().FirstOrDefaultAsync(f => f.FeedbackId == id);

            if (existingFeedback == null || existingFeedback.UserId != currentUser.Id) return Unauthorized();

            ModelState.Remove("UserId");
            ModelState.Remove("User");
            ModelState.Remove("Tour");

            if (ModelState.IsValid)
            {
                feedback.UserId = currentUser.Id;
                feedback.CreatedAt = DateTime.Now; // Or keep original date
                _context.Update(feedback);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Tours", new { id = feedback.TourId });
            }
            return View(feedback);
        }

        // --- DELETE (GET: Confirmation Page) ---
        [Authorize] // Both Admin and Owner might reach here
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var feedback = await _context.Feedbacks
                .Include(f => f.User)
                .FirstOrDefaultAsync(m => m.FeedbackId == id);

            if (feedback == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            bool isAdmin = User.IsInRole("Admin");
            bool isOwner = feedback.UserId == currentUser.Id;

            if (!isAdmin && !isOwner) return Unauthorized();

            return View(feedback);
        }

        // --- DELETE (POST: Actual deletion) ---
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int FeedbackId)
        {
            var feedback = await _context.Feedbacks.FindAsync(FeedbackId);
            if (feedback == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Admin") && feedback.UserId != currentUser.Id) return Unauthorized();

            int tourId = feedback.TourId;
            _context.Feedbacks.Remove(feedback);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Tours", new { id = tourId });
        }
    }
}