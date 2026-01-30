using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TourismWebsite.Models;

namespace TourismWebsite.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Add your app tables here
        public DbSet<Tour> Tours { get; set; } = null!;
        public DbSet<Booking> Bookings { get; set; } = null!;
        public DbSet<Feedback> Feedbacks { get; set; } = null!;
        public DbSet<TouristBooking> TouristBookings { get; set; }  // <-- ADD THIS
    }
}
