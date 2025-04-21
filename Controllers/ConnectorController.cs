// Controllers/ChargePointsController.cs
using ChargingStation.Data;
using ChargingStation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChargingStation.Controllers
{
    [ApiController]
    [Route("api/connectors")]
   
    public class ConnectorController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ConnectorController> _logger;

        public ConnectorController(AppDbContext context, ILogger<ConnectorController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConnectorDto>>> GetAllConnectors()
        {
            var connectors = await _context.Connectors
                .Include(c => c.ChargePoint)
                .Select(c => ToDto(c))
                .ToListAsync();

            return Ok(connectors);
        }

        [HttpGet("{chargePointId}/{connectorId}")]
        public async Task<ActionResult<ConnectorDto>> GetConnectorById(int chargePointId, int connectorId)
        {
            var connector = await _context.Connectors
                .Include(c => c.ChargePoint)
                .FirstOrDefaultAsync(c => c.ChargePointId == chargePointId && c.ConnectorId == connectorId);

            if (connector == null)
            {
                _logger.LogWarning($"Connector {connectorId} not found for charge point {chargePointId}");
                return NotFound(new ProblemDetails
                {
                    Title = "Connector not found",
                    Detail = $"Connector {connectorId} does not exist for charge point {chargePointId}"
                });
            }

            return ToDto(connector);
        }

        [HttpPost]
        public async Task<ActionResult<ConnectorDto>> CreateConnector([FromBody] CreateConnectorDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Vérification que le ConnectorId est valide (si nécessaire)
            if (dto.ConnectorId <= 0)
            {
                return BadRequest("ConnectorId must be a positive integer");
            }

            var chargePoint = await _context.ChargePoints.FindAsync(dto.ChargePointId);
            if (chargePoint == null)
                return NotFound($"ChargePoint {dto.ChargePointId} not found");

            // Vérifier si le ConnectorId existe déjà pour ce ChargePoint
            if (await _context.Connectors.AnyAsync(c =>
                c.ChargePointId == dto.ChargePointId && c.ConnectorId == dto.ConnectorId))
            {
                return Conflict($"Connector with ID {dto.ConnectorId} already exists for this charge point.");
            }

            // Vérifier le type de connecteur
            if (await _context.Connectors.AnyAsync(c =>
                c.ChargePointId == dto.ChargePointId && c.Type == dto.ConnectorType))
            {
                return Conflict($"Connector of type {dto.ConnectorType} already exists for this charge point.");
            }

            var connector = new Connector
            {
                ChargePointId = dto.ChargePointId,
                ConnectorId = dto.ConnectorId, // Ici on assigne la valeur du DTO
                Type = dto.ConnectorType,
                MaxPower = dto.MaxPower,
                Status = dto.Status,
                LastUpdated = DateTime.UtcNow
            };

            _context.Connectors.Add(connector);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetConnectorById),
                new { chargePointId = connector.ChargePointId, connectorId = connector.ConnectorId },
                ToDto(connector));
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<ConnectorDto>> GetConnectorById(int id)
        {
            var connector = await _context.Connectors
                .Include(c => c.ChargePoint)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (connector == null)
            {
                _logger.LogWarning($"Connector {id} not found");
                return NotFound(new ProblemDetails
                {
                    Title = "Connector not found",
                    Detail = $"Connector {id} does not exist"
                });
            }

            return ToDto(connector);
        }

        [HttpDelete("{chargePointId}/{connectorId}")]
        public async Task<IActionResult> DeleteConnector(int chargePointId, int connectorId)
        {
            var connector = await _context.Connectors
                .FirstOrDefaultAsync(c => c.ChargePointId == chargePointId && c.ConnectorId == connectorId);

            if (connector == null)
                return NotFound();

            // Vérification utilisation
            var isInUse = await _context.Transactions
                .AnyAsync(t => t.ConnectorId == connector.Id && t.Status == "Active");

            if (isInUse)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Connector in use",
                    Detail = "Cannot delete connector with active transactions"
                });
            }

            _context.Connectors.Remove(connector);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("by-chargepoint/{chargePointId}")]
        public async Task<ActionResult<IEnumerable<ConnectorDto>>> GetConnectorsByChargePoint(int chargePointId)
        {
            var connectors = await _context.Connectors
                .Where(c => c.ChargePointId == chargePointId)
                .Select(c => ToDto(c))
                .ToListAsync();

            return connectors;
        }

        private static ConnectorDto ToDto(Connector connector)
        {
            return new ConnectorDto
            {
                Id = connector.Id,
                ConnectorId = connector.ConnectorId,
                ChargePointId = connector.ChargePointId,
                Status = connector.Status.ToString(),
                Type = connector.Type.ToString(),
                MaxPower = connector.MaxPower,
                ChargePointName = connector.ChargePoint?.Name ?? "N/A"
            };
        }
    }

    public class CreateConnectorDto
    {
        public int ConnectorId { get; set; }
        public int ChargePointId { get; set; }
      
        public string ConnectorType { get; set; }
        public double MaxPower { get; set; }
        public ChargePointStatus Status { get; internal set; }
    }

  

}