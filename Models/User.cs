using System;
using System.ComponentModel.DataAnnotations;

namespace ChargingStation.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(100)]
        public string Password { get; set; }

        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = "User";
        public ICollection<Reservation> Reservations { get; set; }
        public string? FcmToken { get; set; }
        public DateTime CreatedAt { get; internal set; }
        public string? RfidTag { get; internal set; }
        public DateTime? RfidExpiryDate { get; set; }
        public int? ParentUserId { get; set; }
    }
    }
