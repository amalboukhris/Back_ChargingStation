using System;
using System.ComponentModel.DataAnnotations;

namespace ChargingStation.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }  
        public DateTime DateOfBirth { get; set; }
        public string Role { get; set; } = "User"; 
    }
}
