// Controllers/ChargePointsController.cs
using ChargingStation;
using ChargingStation.Data;
using ChargingStation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using VehicleChargingStation.Dto;

[ApiController]
[Route("api/stations/")]
public class ChargePointsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<ChargePointsController> _logger;
    public ChargePointsController(AppDbContext context, IHubContext<NotificationHub> hubContext, ILogger<ChargePointsController> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _logger = logger;
    }
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChargePointDto>> CreateChargePoint(
     int stationId,
     [FromBody] CreateChargePointDto dto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Validation failed: {@Dto}", dto);
            return BadRequest(ModelState);
        }

        var stationExists = await _context.Stations.AnyAsync(s => s.Id == stationId);
        if (!stationExists)
        {
            return NotFound($"Station {stationId} not found");
        }

        if (await _context.ChargePoints.AnyAsync(cp =>
            cp.StationId == stationId && cp.ChargePointId == dto.ChargePointId))
        {
            return Conflict($"ChargePoint with ID {dto.ChargePointId} already exists in this station");
        }

        var chargePoint = new ChargePoint
        {
            ChargePointId = dto.ChargePointId,
            StationId = stationId,
            Name = dto.Name,
            Model = dto.Model,
            Status = ChargePointStatus.Available,
            LastHeartbeat = DateTime.UtcNow
        };

        try
        {
            _context.ChargePoints.Add(chargePoint);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created ChargePoint {Id} in Station {StationId}",
                chargePoint.Id, stationId);

            return CreatedAtAction(
                nameof(GetChargePoint),
                new { stationId, chargePointId = chargePoint.ChargePointId },
                new ChargePointDto
                {
                    Id = chargePoint.Id,
                    ChargePointId = chargePoint.ChargePointId,
                    Name = chargePoint.Name,
                    Status = chargePoint.Status,
                    Model = chargePoint.Model,
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating ChargePoint");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{chargePointId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChargePointDto>> GetChargePoint(
        int stationId,
        int chargePointId)
    {
        var chargePoint = await _context.ChargePoints
            .FirstOrDefaultAsync(cp =>
                cp.StationId == stationId &&
                cp.ChargePointId == chargePointId);

        if (chargePoint == null)
        {
            return NotFound();
        }

        return new ChargePointDto
        {
            Id = chargePoint.Id,
            ChargePointId = chargePoint.ChargePointId,
            Name = chargePoint.Name,
            Status = chargePoint.Status,
            Model = chargePoint.Model,
        };
    }
    [HttpGet("{chargePointId}/connectors")]
    public async Task<ActionResult<IEnumerable<ConnectorDto>>> GetConnectorsForChargePoint(
    int stationId,
    int chargePointId)
    {
        var chargePoint = await _context.ChargePoints
            .Include(cp => cp.Connectors)
            .FirstOrDefaultAsync(cp =>
               cp.StationId == stationId &&
                cp.ChargePointId == chargePointId);

        if (chargePoint == null)
            return NotFound();

        return Ok(chargePoint.Connectors.Select(c => new ConnectorDto
        {
            Id = c.Id,
            ConnectorId = c.ConnectorId,
            Type = c.Type.ToString(),
            MaxPower = c.MaxPower,
            Status = c.Status.ToString()
        }));
    }
    [HttpPost("{chargePointId}/connectors/{connectorId}/reserve")]
    [Authorize]
    public async Task<ActionResult<ReservationDto>> ReserveConnector(
        [FromRoute] int stationId,
        [FromRoute] int chargePointId,
        [FromRoute] int connectorId,
        [FromBody] ReservationRequestDto request)
    {
        // Validation
        if (request == null) return BadRequest("Request body is required");
        if (request.StartTime >= request.EndTime) return BadRequest("End time must be after start time");

        var chargePoint = await _context.ChargePoints
      .FirstOrDefaultAsync(cp =>
          cp.StationId == stationId &&
          cp.ChargePointId == chargePointId);


        if (chargePoint == null)
        {
            return NotFound(new
            {
                Message = "Borne non trouvée",
                StationId = stationId,
                ChargePointId = chargePointId
            });
        }

        // Recherche du connecteur
        var connector = await _context.Connectors
          .FirstOrDefaultAsync(c =>
              c.ChargePointId == chargePoint.Id &&
              c.ConnectorId == connectorId); // Utiliser ConnectorId ici

        if (connector == null)
        {
            return NotFound(new
            {
                Message = "Connecteur non trouvé",
                ChargePointId = chargePoint.Id,
                ConnectorId = connectorId
            });
        }

        // Vérification des conflits
        var hasConflict = await _context.Reservations
            .AnyAsync(r => r.ConnectorId == connector.Id &&
                          r.Status == "Active" &&
                          request.StartTime < r.EndTime &&
                          request.EndTime > r.StartTime);

        if (hasConflict)
            return Conflict("Le connecteur est déjà réservé pour cette période");

        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
            return Unauthorized("Utilisateur invalide");

        // Création de la réservation
        var reservation = new Reservation
        {
            ChargePointId = connector.ChargePoint.Id,
            ConnectorId = connector.Id,
            UserId = userId,
            StartTime = request.StartTime.ToUniversalTime(),
            EndTime = request.EndTime.ToUniversalTime(),
            Status = "Active",
            ReservationCode = GenerateReservationCode(),
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            await _context.Reservations.AddAsync(reservation);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Réservation créée: {reservation.Id}");

            // Notification temps réel
            await _hubContext.Clients.All.SendAsync("ReservationCreated", new
            {
                ReservationId = reservation.Id,
                ChargePointId = chargePointId,  // Correction ici
                ConnectorId = connectorId,     // Correction ici
                StartTime = reservation.StartTime,
                EndTime = reservation.EndTime
            });


            return Ok(new ReservationDto
            {
                ReservationCode = reservation.ReservationCode,
                ConnectorId = connector.ConnectorId,
                ChargePointId = chargePointId,  // Correction ici
                StationId = stationId          // Correction ici
            });

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création de la réservation");
            return StatusCode(500, "Erreur interne du serveur");
        }
    }
    [HttpGet("reservations")]
    
    [Authorize(Roles = "Admin")] // ou "SuperAdmin", adapte selon ton système
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ReservationDetailsDto>>> GetAllReservations()
    {
        try
        {
            _logger.LogInformation("Récupération de toutes les réservations par un administrateur.");

            var reservations = await _context.Reservations
                .Include(r => r.Connector)
                    .ThenInclude(c => c.ChargePoint)
                        .ThenInclude(cp => cp.Station)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Ok(reservations.Select(r => new ReservationDetailsDto
            {
                Id = r.Id,
                ReservationCode = r.ReservationCode,
                StationId = r.Connector.ChargePoint.StationId,
                ChargePointId = r.Connector.ChargePoint.ChargePointId,
                ConnectorId = r.Connector.ConnectorId,
                UserId = r.UserId,
                UserEmail = r.User?.Email,
                StartTime = r.StartTime,
                EndTime = r.EndTime,
                Status = r.Status,
                CreatedAt = r.CreatedAt
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des réservations par l’admin");
            return StatusCode(500, "Erreur serveur");
        }
    }


    [HttpGet("chargepoints/{chargePointId}/connectors/{connectorId}/reservations")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ReservationDetailsDto>>> GetReservationsForConnector(
      [FromRoute] int stationId,
      [FromRoute] int chargePointId,
      [FromRoute] int connectorId)
    {
        if (!User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
            return Unauthorized();

        var connector = await _context.Connectors
            .Include(c => c.ChargePoint)
            .ThenInclude(cp => cp.Station)
            .FirstOrDefaultAsync(c =>
                c.ChargePoint.StationId == stationId &&
                c.ChargePoint.ChargePointId == chargePointId &&
                c.ConnectorId == connectorId);

        if (connector == null)
            return NotFound("Connecteur introuvable.");

        var reservations = await _context.Reservations
            .Where(r => r.ConnectorId == connector.Id)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return Ok(reservations.Select(r => new ReservationDetailsDto
        {
            Id = r.Id,
            ReservationCode = r.ReservationCode,
            StationId = stationId,
            ChargePointId = chargePointId,
            ConnectorId = connectorId,
            UserId = r.UserId,
            UserEmail = r.User?.Email,
            StartTime = r.StartTime,
            EndTime = r.EndTime,
            Status = r.Status,
            CreatedAt = r.CreatedAt
        }));
    }


    public class ReservationDetailsDto
    {
        public int Id { get; set; }
        public string ReservationCode { get; set; }
        public int StationId { get; set; }
        public int ChargePointId { get; set; }
        public int ConnectorId { get; set; }
        public int UserId { get; set; }
        public string UserEmail { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    [HttpGet("my-reservations")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ReservationDetailsDto>>> GetMyReservations()
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
            return Unauthorized();

        try
        {
            _logger.LogInformation("Tentative de récupération des réservations de l'utilisateur {UserId}", userId);

            var reservations = await _context.Reservations
                .Include(r => r.Connector)
                    .ThenInclude(c => c.ChargePoint)
                        .ThenInclude(cp => cp.Station)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Ok(reservations.Select(r => new ReservationDetailsDto
            {
                Id = r.Id,
                ReservationCode = r.ReservationCode,
                StationId = r.Connector.ChargePoint.StationId,
                ChargePointId = r.Connector.ChargePoint.ChargePointId,
                ConnectorId = r.Connector.ConnectorId,
                UserId = r.UserId,
                UserEmail = r.User?.Email,
                StartTime = r.StartTime,
                EndTime = r.EndTime,
                Status = r.Status,
                CreatedAt = r.CreatedAt
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des réservations");
            return StatusCode(500, "Erreur serveur");
        }
    }

    private static string GenerateReservationCode() =>
        Guid.NewGuid().ToString()[..8].ToUpper();


}


public class CreateChargePointDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "L'ID doit être positif")]
    public int ChargePointId { get; set; } // Identifiant métier (ex: 1, 2, 3...)

    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } // Ex: "Borne principale"

    [Required]
    public string Model { get; set; } // Ex: "ABB Terra 54"


}