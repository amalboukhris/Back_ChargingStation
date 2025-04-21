using ChargingStation.Data;
using ChargingStation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VehicleChargingStation.Dto;

namespace ChargingStation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ReservationsController> _logger;

        public ReservationsController(AppDbContext context, ILogger<ReservationsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetAllReservations()
        {
            try
            {
                var reservations = await _context.Reservations
                    .Include(r => r.Connector)
                        .ThenInclude(c => c.ChargePoint)
                            .ThenInclude(cp => cp.Station)
                    .Include(r => r.User)
                    .AsNoTracking()
                    .Select(r => r.ToReservationDto())
                    .ToListAsync();

                return Ok(reservations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all reservations");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReservationDto>> GetReservationById(int id)
        {
            try
            {
                var reservation = await _context.Reservations
                    .Include(r => r.Connector)
                        .ThenInclude(c => c.ChargePoint)
                            .ThenInclude(cp => cp.Station)
                    .Include(r => r.User)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (reservation == null)
                {
                    return NotFound();
                }

                return reservation.ToReservationDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting reservation with ID {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetReservationsByUser(int userId)
        {
            try
            {
                var reservations = await _context.Reservations
                    .Where(r => r.UserId == userId)
                    .Include(r => r.Connector)
                        .ThenInclude(c => c.ChargePoint)
                            .ThenInclude(cp => cp.Station)
                    .Include(r => r.User)
                    .AsNoTracking()
                    .Select(r => r.ToReservationDto())
                    .ToListAsync();

                return Ok(reservations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting reservations for user {userId}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelReservation(int id)
        {
            try
            {
                var reservation = await _context.Reservations.FindAsync(id);
                if (reservation == null)
                {
                    return NotFound();
                }

                if (reservation.Status == "Cancelled")
                {
                    return BadRequest("Reservation is already cancelled");
                }

                // Vérifier si la réservation est en cours
                if (reservation.StartTime <= DateTime.UtcNow && reservation.EndTime >= DateTime.UtcNow)
                {
                    return BadRequest("Cannot cancel an active reservation");
                }

                reservation.Status = "Cancelled";
                reservation.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error cancelling reservation {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetActiveReservations()
        {
            try
            {
                var now = DateTime.UtcNow;
                var reservations = await _context.Reservations
                    .Where(r => r.Status == "Active" &&
                                r.StartTime <= now &&
                                r.EndTime >= now)
                    .Include(r => r.Connector)
                        .ThenInclude(c => c.ChargePoint)
                            .ThenInclude(cp => cp.Station)
                    .Include(r => r.User)
                    .AsNoTracking()
                    .Select(r => r.ToReservationDto())
                    .ToListAsync();

                return Ok(reservations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active reservations");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}

