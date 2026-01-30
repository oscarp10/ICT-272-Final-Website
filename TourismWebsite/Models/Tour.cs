using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace TourismWebsite.Models
{
    public class Tour
    {
        public int TourId { get; set; }

        [Required]
        public string Title { get; set; } = null!;

        [Required]
        public string Description { get; set; } = null!;

        [Required]
        public decimal Price { get; set; }

        public DateTime AvailableDate { get; set; }
        public int MaxGroupSize { get; set; }

        // Images stored as URLs
        public string? Image1 { get; set; }
        public string? Image2 { get; set; }
        public string? Image3 { get; set; }

        // Used only for upload (NOT stored in DB)
        [NotMapped] public IFormFile? UploadImage1 { get; set; }
        [NotMapped] public IFormFile? UploadImage2 { get; set; }
        [NotMapped] public IFormFile? UploadImage3 { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        // New category
        [Required]
        public string Category { get; set; } = string.Empty;

        public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    }
}
