using System;
using System.Collections.Generic;

namespace ChargingStation.Models
{
    public class Client : User
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}