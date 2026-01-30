using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using TourismWebsite.Models;

namespace TourismWebsite.Models
{
    public class Booking
    {
        public int BookingId { get; set; }

        public int TourId { get; set; }
        public Tour? Tour { get; set; }

        public string? UserId { get; set; }
        public IdentityUser? User { get; set; }

        [Required]
        public string? TouristName { get; set; }

        [Required]
        public string? TouristEmail { get; set; }

        [Required]
        public string? TouristPhone { get; set; }

        [Required]
        [Range(1, 100)]
        public int NumPeople { get; set; } = 1;

        public decimal TotalPrice { get; set; }

        public DateTime BookingDate { get; set; } = DateTime.Now;

        [Required]
        public string Status { get; set; } = "Pending"; // Pending / Confirmed / Rejected
    }
}
