using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourismWebsite.Data;
using TourismWebsite.Models;

[Authorize]
public class ToursController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public ToursController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // GET: Tours
    [AllowAnonymous]
    public async Task<IActionResult> Index(string? category, string? sortOrder, string? searchString)
    {
        // 1. Prepare Categories for the dropdown
        var categories = await _context.Tours
            .Select(t => t.Category)
            .Distinct()
            .ToListAsync();

        ViewBag.Categories = categories;
        ViewBag.SelectedCategory = category ?? "All";
        ViewBag.CurrentSort = sortOrder;

        // 2. Build Query with Include for Ratings
        var toursQuery = _context.Tours.Include(t => t.Feedbacks).AsQueryable();

        // 3. Search Logic
        if (!string.IsNullOrEmpty(searchString))
        {
            toursQuery = toursQuery.Where(t => t.Title.Contains(searchString) ||
                                              t.Category.Contains(searchString) ||
                                              t.Description.Contains(searchString));
        }

        // 4. Category Filter
        if (!string.IsNullOrEmpty(category) && category != "All")
        {
            toursQuery = toursQuery.Where(t => t.Category == category);
        }

        // 5. Sorting Logic (WITH SQLITE FIX: Casting to Double)
        toursQuery = sortOrder switch
        {
            "price_asc" => toursQuery.OrderBy(t => (double)t.Price),
            "price_desc" => toursQuery.OrderByDescending(t => (double)t.Price),
            "best_reviewed" => toursQuery.OrderByDescending(t => t.Feedbacks.Any()
                                ? (double)t.Feedbacks.Average(f => f.Rating)
                                : 0.0),
            _ => toursQuery.OrderByDescending(t => t.CreatedAt) // Default
        };

        var tours = await toursQuery.ToListAsync();

        // 6. Pre-calculate Ratings for the View Dictionary
        ViewBag.Ratings = tours.ToDictionary(
            t => t.TourId,
            t => t.Feedbacks.Any() ? t.Feedbacks.Average(f => f.Rating) : 0.0
        );

        return View(tours);
    }

    // GET: Tours/Details/5
    [AllowAnonymous]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var tour = await _context.Tours
            .Include(t => t.Feedbacks)
                .ThenInclude(f => f.User)
            .FirstOrDefaultAsync(t => t.TourId == id);

        if (tour == null) return NotFound();

        return View(tour);
    }

    // GET: Tours/Create
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = await GetDistinctCategories();
        return View(new Tour());
    }

    // POST: Tours/Create
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Tour tour)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await GetDistinctCategories();
            return View(tour);
        }

        tour.CreatedAt = DateTime.Now;

        if (tour.UploadImage1 != null) tour.Image1 = SaveImage(tour.UploadImage1);
        if (tour.UploadImage2 != null) tour.Image2 = SaveImage(tour.UploadImage2);
        if (tour.UploadImage3 != null) tour.Image3 = SaveImage(tour.UploadImage3);

        _context.Tours.Add(tour);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: Tours/Edit/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var tour = await _context.Tours.FindAsync(id);
        if (tour == null) return NotFound();

        ViewBag.Categories = await GetDistinctCategories();
        return View(tour);
    }

    // POST: Tours/Edit/5
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Tour tour)
    {
        if (id != tour.TourId) return NotFound();

        var existing = await _context.Tours.FindAsync(id);
        if (existing == null) return NotFound();

        // Map updated values
        existing.Title = tour.Title;
        existing.Description = tour.Description;
        existing.Price = tour.Price;
        existing.AvailableDate = tour.AvailableDate;
        existing.MaxGroupSize = tour.MaxGroupSize;
        existing.Category = tour.Category;

        if (tour.UploadImage1 != null) existing.Image1 = SaveImage(tour.UploadImage1);
        if (tour.UploadImage2 != null) existing.Image2 = SaveImage(tour.UploadImage2);
        if (tour.UploadImage3 != null) existing.Image3 = SaveImage(tour.UploadImage3);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: Tours/Delete/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var tour = await _context.Tours.FirstOrDefaultAsync(t => t.TourId == id);
        if (tour == null) return NotFound();
        return View(tour);
    }

    // POST: Tours/Delete
    [Authorize(Roles = "Admin")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var tour = await _context.Tours.FindAsync(id);
        if (tour != null)
        {
            _context.Tours.Remove(tour);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    // Helpers
    private async Task<List<string>> GetDistinctCategories()
    {
        return await _context.Tours.Select(t => t.Category).Distinct().ToListAsync();
    }

    private string? SaveImage(IFormFile? file)
    {
        if (file == null) return null;
        var uploads = Path.Combine(_env.WebRootPath, "images");
        Directory.CreateDirectory(uploads);
        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
        var filePath = Path.Combine(uploads, fileName);
        using var stream = new FileStream(filePath, FileMode.Create);
        file.CopyTo(stream);
        return "/images/" + fileName;
    }
}