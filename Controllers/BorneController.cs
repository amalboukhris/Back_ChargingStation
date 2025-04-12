using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChargingStation.Models;
using ChargingStation.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace VehicleChargingStation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BorneController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        public BorneController(AppDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        //[HttpPost]
        //public async Task<ActionResult<Borne>> PostBorne([FromBody] Borne borne)
        //{
        //    if (borne == null)
        //    {
        //        return BadRequest("Borne is null.");
        //    }


        //    var existingBorne = await _context.Bornes.FindAsync(borne.Id);
        //    if (existingBorne != null)
        //    {
        //        return Conflict("A Borne with the same ID already exists.");
        //    }

        //    var chargingStation = await _context.ChargingStations.FindAsync(borne.ChargingStationId);
        //    if (chargingStation == null)
        //    {
        //        return BadRequest("Charging Station with the given ID does not exist.");
        //    }

        //    borne.ChargingStation = chargingStation;

        //    _context.Bornes.Add(borne);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction(nameof(GetBorne), new { id = borne.Id }, borne);
        //}

        //[HttpPost]
        //public async Task<IActionResult> PostBorne([FromBody] BorneCreateDto borneDto)
        //{
        //    if (borneDto == null)
        //    {
        //        return BadRequest("Données invalides");
        //    }

        //    var borne = new Borne
        //    {
        //        ChargingStationId = borneDto.ChargingStationId,
        //         Nom = borneDto.Nom
        //    };

        //    _context.Bornes.Add(borne);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction(nameof(GetBorne), new { id = borne.Id }, borne);
        //}

        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<Borne>>> GetBornes()
        //{
        //    return await _context.Bornes.Include(b => b.ChargingStation).ToListAsync();
        //}


        //[HttpGet("{id}")]
        //public async Task<ActionResult<Borne>> GetBorne(int id)
        //{
        //    var borne = await _context.Bornes.Include(b => b.ChargingStation)
        //                                     .FirstOrDefaultAsync(b => b.Id == id);

        //    if (borne == null)
        //    {
        //        return NotFound();
        //    }

        //    return borne;
        //}



        // Ajouter une borne
        [HttpPost]
        public async Task<IActionResult> AddBorne([FromBody] BorneCreateDto borneDto)
        {
            var borne = new Borne
            {
                ChargingStationId = borneDto.ChargingStationId,
                Nom = borneDto.Nom,
                Etat = "Disponible" // Lors de la création, la borne est disponible
            };

            _context.Bornes.Add(borne);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBorne), new { id = borne.Id }, borne);
        }

        // Récupérer une borne par son ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBorne(int id)
        {
            var borne = await _context.Bornes
                .Include(b => b.ChargingStation)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (borne == null)
                return NotFound();

            return Ok(borne);
        }

        [HttpPost("reserve/{id}")]
        public async Task<IActionResult> ReserveBorne(int id)
        {
            try
            {
                var borne = await _context.Bornes.FindAsync(id);
                if (borne == null)
                    return NotFound("Borne introuvable.");

                if (borne.Etat == "Occupée")
                    return BadRequest("La borne est déjà occupée.");

                var userId = GetCurrentUserId();
                if (userId <= 0) // Vérifiez si userId est un entier valide (positif)
                {
                    return Unauthorized("Utilisateur non authentifié.");
                }

                borne.Etat = "Occupée";
                borne.ReservationUserId = userId;

                await _context.SaveChangesAsync();
                return Ok("Borne réservée avec succès.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }


        [HttpPost("release/{id}")]
        public async Task<IActionResult> ReleaseBorne(int id)
        {
            var borne = await _context.Bornes.FindAsync(id);
            if (borne == null)
                return NotFound("Borne introuvable.");

            var userId = GetCurrentUserId();
            if (borne.ReservationUserId != userId)
                return Forbid("Vous n'êtes pas autorisé à libérer cette borne.");

            borne.Etat = "Disponible";
            borne.ReservationUserId = null;

            await _context.SaveChangesAsync();

            var notification = new NotificationData
            {
                Message = $"La borne {borne.Nom} est maintenant disponible.",
                Date = DateTime.UtcNow,
                ClientId = userId
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Envoi à l'utilisateur qui a libéré la borne (stocké en base)
            await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveGlobalNotification", new
            {
                Id = notification.Id,
                Message = "Vous avez libéré la borne " + borne.Nom,
                Date = notification.Date.ToString("g"),
                IsGlobal = false
            });

            // Envoi à tous les utilisateurs connectés
            await _hubContext.Clients.All.SendAsync("ReceiveGlobalNotification", new
            {
                Id = notification.Id,
                Message = $"La borne {borne.Nom} est maintenant disponible.",
                Date = notification.Date.ToString("g"),
                IsGlobal = true
            });

            return Ok(new
            {
                Message = "Borne libérée avec succès",
                NotificationId = notification.Id
            });
        }

        [HttpGet("user/{userId}/unread-notifications")]
        public async Task<IActionResult> GetUnreadNotifications(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.ClientId == userId && !n.IsRead)
                .OrderByDescending(n => n.Date)
                .ToListAsync();

            // Marquer comme lues
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }
            await _context.SaveChangesAsync();

            return Ok(notifications);
        }
        [HttpGet("notifications/{userId}")]
        public async Task<IActionResult> GetUserNotifications(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.ClientId == userId)
                .OrderByDescending(n => n.Date)
                .ToListAsync();

            return Ok(notifications);
        }





        // Récupérer les bornes disponibles par station
        [HttpGet("station/{stationId}/available")]
        public async Task<IActionResult> GetAllBornesByStation(int stationId)
        {
            var bornes = await _context.Bornes
                .Where(b => b.ChargingStationId == stationId)
                .ToListAsync();

            if (bornes == null || !bornes.Any())
            {
                return NotFound($"Aucune borne trouvée pour la station avec l'ID {stationId}.");
            }

            return Ok(bornes);
        }


        // Récupérer toutes les bornes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Borne>>> GetAllBornes()
        {
            var bornes = await _context.Bornes.Include(b => b.ChargingStation).ToListAsync();
            return Ok(bornes);
        }
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                throw new UnauthorizedAccessException("Utilisateur non authentifié ou ID utilisateur manquant.");
            }

            // Assurez-vous que la valeur de userIdClaim est bien un nombre sous forme de chaîne
            if (int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            else
            {
                throw new UnauthorizedAccessException("ID utilisateur invalide.");
            }
        }





    } }
public class NotificationDto
{
    public int Id { get; set; }
    public string Message { get; set; }
    public string Date { get; set; }
    public bool IsRead { get; set; }
}

public class BorneCreateDto
        {
            public int ChargingStationId { get; set; }
            public string Nom { get; set; }
            
        }


        public class BorneDto
{
    public int Id { get; set; }
    public string Nom { get; set; }
    public string Etat { get; set; } // "Disponible", "Occupée", "En maintenance"
}


    
