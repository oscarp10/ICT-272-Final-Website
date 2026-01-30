using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TourismWebsite.Models;
using TourismWebsite.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace TourismWebsite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? sortOrder, string? searchString, string? category)
        {
            // 1. Start with the query and INCLUDE Feedbacks so ratings show up
            var toursQuery = _context.Tours.Include(t => t.Feedbacks).AsQueryable();

            // 2. Filter by Search String
            if (!string.IsNullOrEmpty(searchString))
            {
                toursQuery = toursQuery.Where(t => t.Title.Contains(searchString) || t.Description.Contains(searchString));
            }

            // 3. Filter by Category
            if (!string.IsNullOrEmpty(category) && category != "All")
            {
                toursQuery = toursQuery.Where(t => t.Category == category);
            }

            // 4. Sort Logic (Including Best Reviewed)
            toursQuery = sortOrder switch
            {
                "price_asc" => toursQuery.OrderBy(t => (double)t.Price),
                "price_desc" => toursQuery.OrderByDescending(t => (double)t.Price),
                // Cast the Average result to double for SQLite compatibility
                "best_reviewed" => toursQuery.OrderByDescending(t => t.Feedbacks.Any()
                                    ? (double)t.Feedbacks.Average(f => f.Rating)
                                    : 0.0),
                _ => toursQuery.OrderByDescending(t => t.CreatedAt)
            };
            var tours = await toursQuery.ToListAsync();

            // 5. Pre-calculate Ratings for the View
            ViewBag.Ratings = tours.ToDictionary(
                t => t.TourId,
                t => t.Feedbacks.Any() ? t.Feedbacks.Average(f => (double)f.Rating) : 0.0
            );

            // Keep categories list for the dropdown
            ViewBag.Categories = await _context.Tours.Select(t => t.Category).Distinct().ToListAsync();

            return View(tours);
        }

        public IActionResult PrivacyPolicy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Terms()
        {
            return View();
        }

        public IActionResult AdPolicy()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SubmitContact(string Name, string Email, string Message)
        {
            // Save to database or send email here
            TempData["Success"] = "Your message has been sent successfully!";
            return RedirectToAction("Contact");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

       
    }
}
