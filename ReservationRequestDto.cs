// Controllers/ChargePointsController.cs
using System.ComponentModel.DataAnnotations;

namespace ChargingStation
{
    public class ReservationRequestDto
    {

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

}