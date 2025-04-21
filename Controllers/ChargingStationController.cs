// Controllers/StationsController.cs
using ChargingStation.Data;
using ChargingStation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using VehicleChargingStation.Dto;

[ApiController]
[Route("api/stations")]
public class StationsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<StationsController> _logger; // Déclaration correcte

    public StationsController(AppDbContext context, ILogger<StationsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<StationDto>> CreateStation([FromBody] CreateStationDto dto)
    {
        // 1. Validation du DTO
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Validation failed for CreateStation: {@Dto}", dto);
            return BadRequest(ModelState);
        }

        // 2. Vérifier si la station existe déjà (par nom ou coordonnées)
        bool stationExists = await _context.Stations
            .AnyAsync(s => s.Name == dto.Name ||
                          (s.Latitude == dto.Latitude && s.Longitude == dto.Longitude));

        if (stationExists)
        {
            _logger.LogWarning("Station already exists: {Name}", dto.Name);
            return Conflict($"Station '{dto.Name}' or coordinates already exist.");
        }

        // 3. Création de la station
        var station = new Station
        {
            Name = dto.Name,
            Address = dto.Address,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
           
        };

        // 4. Sauvegarde en base
        try
        {
            _context.Stations.Add(station);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Station created: {Id} - {Name}", station.Id, station.Name);

            // 5. Retourne la réponse avec l'URL de la nouvelle ressource
            return CreatedAtAction(
                nameof(GetStationById),
                new { id = station.Id },
                new StationDto
                {
                    Id = station.Id,
                    Name = station.Name,
                    Address = station.Address,
                    Latitude = station.Latitude,
                    Longitude = station.Longitude,
                   
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating station");
            return StatusCode(500, "Internal server error");
        }
    }

    private object GetStationById()
    {
        throw new NotImplementedException();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StationDto>>> GetStations(
       )
    {
        var query = _context.Stations
            .Include(s => s.ChargePoints)
                .ThenInclude(cp => cp.Connectors)
            .AsQueryable();

       

        var stations = await query
            .Select(s => new StationDto
            {
                Id = s.Id,
                Name = s.Name,
                Address = s.Address,
                Latitude = s.Latitude,
                Longitude = s.Longitude,
                AvailableChargePoints = s.ChargePoints.Count(cp =>
                    cp.Status == ChargePointStatus.Available),
                TotalChargePoints = s.ChargePoints.Count
            })
            .ToListAsync();

        return Ok(stations);
    }
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<StationSearchDto>>> SearchStations(
      
        [FromQuery] string? name = null) // <-- Ajout ici
    {
        var query = _context.Stations
            .Include(s => s.ChargePoints)
                .ThenInclude(cp => cp.Connectors)
            .AsQueryable();

        // Filtre par nom
        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(s => s.Name.ToLower().Contains(name.ToLower()));
        }

        
        var stations = await query
            .Select(s => new StationSearchDto
            {
                Id = s.Id,
                Name = s.Name,
                Address = s.Address,
                AvailableChargePoints = s.ChargePoints.Count(cp =>
                    cp.Status == ChargePointStatus.Available),
                Connectors = s.ChargePoints
                    .SelectMany(cp => cp.Connectors)
                    .Select(c => new ConnectorInfoDto
                    {
                        Type = c.Type.ToString(),
                        MaxPower = c.MaxPower,
                        Status = c.Status.ToString()
                    })
                    .ToList()
            })
            .ToListAsync();

        return Ok(stations);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StationDto>> GetStationById(int id)
    {
        var station = await _context.Stations
            .Include(s => s.ChargePoints)
            .ThenInclude(cp => cp.Connectors)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (station == null)
        {
            return NotFound();
        }

        return Ok(new StationDto
        {
            Id = station.Id,
            Name = station.Name,
            Address = station.Address,
            Latitude = station.Latitude,
            Longitude = station.Longitude,
            AvailableChargePoints = station.ChargePoints.Count(cp => cp.Status == ChargePointStatus.Available),
            TotalChargePoints = station.ChargePoints.Count
        });
    }

}// Pour la recherche avancée
public class StationSearchDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int AvailableChargePoints { get; set; }
    public List<ConnectorInfoDto> Connectors { get; set; } = new();
}

public class ConnectorInfoDto
{
    public string Type { get; set; } = string.Empty;
    public double MaxPower { get; set; }
    public string Status { get; set; } = string.Empty;
}

// Pour la réservation

public class CreateStationDto
{
    [Required(ErrorMessage = "Le nom est obligatoire")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Le nom doit contenir entre 3 et 100 caractères")]
    public string Name { get; set; }

    [Required(ErrorMessage = "L'adresse est obligatoire")]
    public string Address { get; set; }

    [Range(-90, 90, ErrorMessage = "La latitude doit être entre -90 et 90")]
    public double Latitude { get; set; }

    [Range(-180, 180, ErrorMessage = "La longitude doit être entre -180 et 180")]
    public double Longitude { get; set; }

    
    
}
