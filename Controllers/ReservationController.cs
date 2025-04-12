//using Microsoft.AspNetCore.Mvc;
//using ChargingStation.Models;
//using Microsoft.EntityFrameworkCore;
//using System.Linq;
//using System.Threading.Tasks;
//using ChargingStation.Data;

//namespace VehicleChargingStation.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class ReservationController : ControllerBase
//    {
//        private readonly AppDbContext _context;

//        public ReservationController(AppDbContext context)
//        {
//            _context = context;
//        }

//        //[HttpGet("vehicles")]
//        //public async Task<IActionResult> GetVehicles()
//        //{
//        //    var vehicles = await _context.Vehicles
//        //        .Select(v => new { v.Id, v.ModelName })
//        //        .ToListAsync();

//        //    return Ok(vehicles);
//        //}



//        //[HttpPost("reserve")]
//        //public async Task<IActionResult> CreateReservation([FromBody] ReservationCreateDto reservationDto)
//        //{
//        //    if (reservationDto == null)
//        //    {
//        //        return BadRequest("Invalid reservation data.");
//        //    }

//        //    var reservation = new Reservation
//        //    {
//        //        VehicleId = reservationDto.VehicleId,
//        //        ChargingStationId = reservationDto.ChargingStationId
//        //    };

//        //    _context.Reservations.Add(reservation);
//        //    await _context.SaveChangesAsync();

//        //    return Ok(new { message = "Reservation successfully created." });
//        //}
//        [ApiController]
//        [Route("api/[controller]")]
//        public class ReservationsController : ControllerBase
//        {
//            private readonly AppDbContext _context;

//            public ReservationsController(AppDbContext context)
//            {
//                _context = context;
//            }

//            //    [HttpPost]
//            //    public async Task<IActionResult> CreateReservation([FromBody] Reservation reservation)
//            //    {
//            //        // Vérification de la disponibilité
//            //        bool exists = await _context.Reservations
//            //            .AnyAsync(r => r.ChargingPointId == reservation.ChargingPointId &&
//            //                           r.ReservationDate.Date == reservation.ReservationDate.Date);

//            //        if (exists)
//            //        {
//            //            return Conflict("La borne est déjà réservée à cette date.");
//            //        }

//            //        _context.Reservations.Add(reservation);
//            //        await _context.SaveChangesAsync();

//            //        return Ok(reservation);
//            //    }
//            }

//            public class ReservationDto
//            {
//                public int BorneId { get; set; }
//                public DateTime DateDebut { get; set; }
//                public DateTime DateFin { get; set; }
//                public string? ClientId { get; set; } // facultatif
//            }

//            public class ReservationCreateDto
//            {
//                public int VehicleId { get; set; }
//                public int ChargingStationId { get; set; }
//            }

//        }
//    }
