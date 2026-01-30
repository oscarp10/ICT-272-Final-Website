using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TourismWebsite.Models;

namespace TourismWebsite.Models
{
    public class Feedback
    {
        public int FeedbackId { get; set; }

        [Required]
        public int TourId { get; set; }

        [ForeignKey("TourId")]
        public virtual Tour? Tour { get; set; }

        [Required]
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual IdentityUser? User { get; set; }

        [Required(ErrorMessage = "Please write a short comment about your experience.")]
        [StringLength(500, ErrorMessage = "Comments cannot exceed 500 characters.")]
        public string Comment { get; set; }

        [Required, Range(1, 5)]
        public int Rating { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}