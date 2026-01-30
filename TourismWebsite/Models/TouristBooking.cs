using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourismWebsite.Models
{
    public class TouristBooking
    {
        [Key]
        public int BookingId { get; set; }

        [Required]
        public int TourId { get; set; }

        [ForeignKey("TourId")]
        public virtual Tour? Tour { get; set; }

        [Required]
        public string TouristName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string TouristEmail { get; set; } = string.Empty;

        [Required, Phone]
        public string TouristPhone { get; set; } = string.Empty;

        [Required]
        [Range(1, 100)]
        public int NumPeople { get; set; } 

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public DateTime BookingDate { get; set; } = DateTime.Now;

        public string Status { get; set; } = "Pending";

        [Required]
        public string UserId { get; set; } = string.Empty;
    }
}
