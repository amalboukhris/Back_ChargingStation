// Controllers/OcppController.cs
using ChargingStation.Data;
using ChargingStation.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text;
using ChargingStation.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ChargingStation.Hubs;
using System.Net;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

[Route("api/ocpp")]
[ApiController]
public class OcppController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IChargePointService _chargePointService;
    private readonly ILogger<OcppController> _logger;
    private readonly IHubContext<ChargingHub, IChargingHubClient> _hubContext;
    private readonly INotificationService _notificationService;
    private readonly IFirmwareService _firmwareService;
    private readonly IOcppProtocolService _ocppService;

    public OcppController(
        AppDbContext context,
        IChargePointService chargePointService,
        ILogger<OcppController> logger,
        IHubContext<ChargingHub, IChargingHubClient> hubContext,
        INotificationService notificationService,
        IFirmwareService firmwareService,
        IOcppProtocolService ocppService)
    {
        _context = context;
        _chargePointService = chargePointService;
        _logger = logger;
        _hubContext = hubContext;
        _notificationService = notificationService;
        _firmwareService = firmwareService;
        _ocppService = ocppService;
    }


    [HttpPost("{chargePointId}/remote-start")]
    public async Task<IActionResult> RemoteStart(
        int chargePointId,
        [FromBody] RemoteStartRequest request)
    {
        var result = await _ocppService.RemoteStartTransaction(
            chargePointId,
            request.ConnectorId);

        return Ok(new { status = result ? "Accepted" : "Rejected" });
    }

    // Heartbeat
    [HttpGet("heartbeat")] // Route relative
    public string HandleHeartbeat(int chargePointId)
    {
        return JsonSerializer.Serialize(new object[]
        {
            3,
            Guid.NewGuid().ToString(),
            new { currentTime = DateTime.UtcNow.ToString("o") }
        });
    }
    

    [HttpGet("{chargePointId}/ws")]
    public async Task HandleOcppConnection(int chargePointId)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await HttpContext.Response.WriteAsync("This endpoint requires WebSocket connection");
            return;
        }

        try
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            _logger.LogInformation($"WebSocket connection established for charge point {chargePointId}");

            var chargePoint = await _chargePointService.RegisterConnectionAsync(chargePointId);
            if (chargePoint == null)
            {
                _logger.LogWarning($"Charge point {chargePointId} not found");
                HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }

            try
            {
                await ProcessOcppMessages(webSocket, chargePoint);
            }
            finally
            {
                await _chargePointService.MarkAsDisconnectedAsync(chargePointId);
                _logger.LogInformation($"WebSocket connection closed for charge point {chargePointId}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error handling WebSocket connection for charge point {chargePointId}");
            HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
    }

    private async Task ProcessOcppMessages(WebSocket webSocket, ChargePoint chargePoint)
    {
        var buffer = new byte[1024 * 4];
        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!result.CloseStatus.HasValue)
        {
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            _logger.LogInformation("Received OCPP: {Message}", message);

            try
            {
                // Parse le message pour extraire le messageId
                using var doc = JsonDocument.Parse(message);
                var messageId = doc.RootElement[1].GetString();

                var response = await ProcessOcppMessage(chargePoint, doc, messageId);
                if (response != null)
                {
                    await webSocket.SendAsync(
                        new ArraySegment<byte>(Encoding.UTF8.GetBytes(response)),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing OCPP message");
            }

            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
    }
    private async Task<string> ProcessOcppMessage(ChargePoint chargePoint, JsonDocument doc, string messageId)
    {
        try
        {
            int messageType = doc.RootElement[0].GetInt32();
            string action = messageType == 2 ? doc.RootElement[2].GetString() : null;
            JsonElement payload = messageType == 2 ? doc.RootElement[3] : new JsonElement();

            return action switch
            {
                // Messages OCPP de base
                "BootNotification" => await HandleBootNotification(chargePoint, messageId),
                "StatusNotification" => await HandleStatusNotification(chargePoint, payload, messageId),
                "StartTransaction" => await HandleStartTransaction(chargePoint, payload, messageId),
                "StopTransaction" => await HandleStopTransaction(chargePoint, payload, messageId),
                "MeterValues" => await HandleMeterValues(chargePoint, payload, messageId),
                "Heartbeat" => HandleHeartbeat(messageId),

                // Messages OCPP avancés (si implémentés)
                "Authorize" => await HandleAuthorize(chargePoint, payload, messageId),
                "DataTransfer" => await HandleDataTransfer(chargePoint, payload, messageId),
                "DiagnosticsStatusNotification" => await HandleDiagnosticsStatus(chargePoint, payload, messageId),
                "FirmwareStatusNotification" => await HandleFirmwareStatus(chargePoint, payload, messageId),

                // Cas par défaut
                _ => CreateCallError(messageId, "NotSupported",
                    $"Action '{action}' is not supported. Supported actions: " +
                    "BootNotification, StatusNotification, StartTransaction, StopTransaction, " +
                    "MeterValues, Heartbeat, Authorize")
            };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "OCPP message parsing failed");
            return CreateCallError(messageId, "FormationViolation", "Invalid message format");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OCPP processing error");
            return CreateCallError(messageId, "InternalError", "Server processing error");
        }
    }
    private async Task<string> HandleBootNotification(ChargePoint chargePoint, string messageId)
    {
        chargePoint.LastHeartbeat = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return CreateCallResult(messageId, new
        {
            status = "Accepted",
            currentTime = DateTime.UtcNow.ToString("o"),
            interval = 300
        });
    }

    private string HandleHeartbeat(string messageId)
    {
        return CreateCallResult(messageId, new
        {
            currentTime = DateTime.UtcNow.ToString("o")
        });
    }

    private async Task<string> HandleStartTransaction(ChargePoint chargePoint, JsonElement payload, string messageId)
    {
        var transaction = new Transaction
        {
            ChargePointId = chargePoint.Id,
            ConnectorId = payload.GetProperty("connectorId").GetInt32(),
            IdTag = payload.GetProperty("idTag").GetString(),
            MeterStart = payload.GetProperty("meterStart").GetInt32(),
            StartTime = DateTime.UtcNow,
            Status = "Active"
        };

        await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();

        return CreateCallResult(messageId, new
        {
            transactionId = transaction.Id,
            idTagInfo = new { status = "Accepted" }
        });
    }

    private async Task<string> HandleStatusNotification(ChargePoint chargePoint, JsonElement payload, string messageId)
    {
        var connectorId = payload.GetProperty("connectorId").GetInt32();
        var status = payload.GetProperty("status").GetString();
        var errorCode = payload.GetProperty("errorCode").GetString();

        var connector = await _context.Connectors
            .FirstOrDefaultAsync(c => c.ChargePointId == chargePoint.Id && c.ConnectorId == connectorId);

        if (connector == null)
        {
            connector = new Connector
            {
                ConnectorId = connectorId,
                ChargePointId = chargePoint.Id,
                Status = MapOcppStatus(status),
                LastUpdated = DateTime.UtcNow
            };
            await _context.Connectors.AddAsync(connector);
        }
        else
        {
            connector.Status = MapOcppStatus(status);
            connector.LastUpdated = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // Utilisation du DTO correct
        await _hubContext.Clients.Group(chargePoint.Id.ToString())
            .ConnectorStatusChanged(new ConnectorStatusDto
            {
                ChargePointId = chargePoint.ChargePointId,
                ConnectorId = connectorId,
                Status = status,
                Timestamp = DateTime.UtcNow
            });

        return CreateCallResult(messageId, new { });
    }
    private async Task<string> HandleStopTransaction(ChargePoint chargePoint, JsonElement payload, string messageId)
    {
        var transactionId = payload.GetProperty("transactionId").GetInt32();
        var meterStop = payload.GetProperty("meterStop").GetInt32();
        var timestamp = payload.GetProperty("timestamp").GetDateTime();

        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == transactionId && t.ChargePointId == chargePoint.Id);

        if (transaction == null)
            return CreateCallError(messageId, "GenericError", "Transaction not found");

        transaction.MeterStop = meterStop;
        transaction.EndTime = timestamp;
        transaction.Status = "Completed";
        await _context.SaveChangesAsync();

        // Envoyer notification FCM
        var user = await _context.Reservations
     .Where(r => r.Transactions.Any(t => t.Id == transactionId))
     .Select(r => r.User)
     .FirstOrDefaultAsync();

        if (user?.FcmToken != null)
        {
            await _notificationService.SendAsync(user.FcmToken,
                "Charging Complete",
                $"Session ended on {chargePoint.Name}. Energy used: {(meterStop - transaction.MeterStart) / 1000.0} kWh");
        }

        return CreateCallResult(messageId, new { });
    }

    private async Task<string> HandleMeterValues(ChargePoint chargePoint, JsonElement payload, string messageId)
    {
        var connectorId = payload.GetProperty("connectorId").GetInt32();
        var transactionId = payload.GetProperty("transactionId").GetInt32();
        var meterValues = payload.GetProperty("meterValue").EnumerateArray();

        foreach (var mv in meterValues)
        {
            var timestamp = mv.GetProperty("timestamp").GetDateTime();
            var sampledValues = mv.GetProperty("sampledValue").EnumerateArray();

            foreach (var sv in sampledValues)
            {
                var value = sv.GetProperty("value").GetString() ?? string.Empty;
                var measurand = sv.GetProperty("measurand").GetString();
                var unit = sv.GetProperty("unit").GetString();

                await _context.MeterValues.AddAsync(new MeterValue
                {
                    ChargePointId = chargePoint.Id,
                    ConnectorId = connectorId,
                    TransactionId = transactionId,
                    Timestamp = timestamp,
                    Value = decimal.Parse(value),
                    Measurand = measurand,
                    Unit = unit
                });
            }
        }

        await _context.SaveChangesAsync();
        return CreateCallResult(messageId, new { });
    }
 

    private async Task<string> HandleAuthorize(ChargePoint chargePoint, JsonElement payload, string messageId)
    {
        var idTag = payload.GetProperty("idTag").GetString();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.RfidTag == idTag);

        return CreateCallResult(messageId, new
        {
            idTagInfo = new
            {
                status = user != null ? "Accepted" : "Invalid",
                expiryDate = user?.RfidExpiryDate.ToString(),
                parentIdTag = user?.ParentUserId.ToString()
            }
        });
    }

    private Task<string> HandleDataTransfer(ChargePoint chargePoint, JsonElement payload, string messageId)
    {
        // Implémentation basique pour la démonstration
        return Task.FromResult(CreateCallResult(messageId, new
        {
            status = "Accepted",
            data = "{}"
        }));
    }

    private async Task<string> HandleDiagnosticsStatus(ChargePoint chargePoint, JsonElement payload, string messageId)
    {
        var status = payload.GetProperty("status").GetString();

        _logger.LogInformation($"Diagnostics status for {chargePoint.ChargePointId}: {status}");

        // Stocker le statut si pertinent
        chargePoint.DiagnosticsStatus = status;
        chargePoint.LastDiagnosticsTime = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return CreateCallResult(messageId, new { });
    }


    private string CreateCallResult(string messageId, object payload)
    {
        return JsonSerializer.Serialize(new object[]
        {
            3, // CallResult
            messageId,
            payload
        });
    }

    private async Task<string> HandleFirmwareStatus(ChargePoint chargePoint, JsonElement payload, string messageId)
    {
        var status = payload.GetProperty("status").GetString();

        try
        {
            await _firmwareService.UpdateFirmwareStatusAsync(chargePoint.Id, status);
            _logger.LogInformation($"Firmware update status from {chargePoint.ChargePointId}: {status}");
            return CreateCallResult(messageId, new { });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update firmware status");
            return CreateCallError(messageId, "InternalError", "Failed to update firmware status");
        }
    }


    // Méthode utilitaire pour mapper les statuts OCPP
    private ChargePointStatus MapOcppStatus(string ocppStatus)
    {
        return ocppStatus switch
        {
            "Available" => ChargePointStatus.Available,
            "Preparing" => ChargePointStatus.Preparing,
            "Charging" => ChargePointStatus.Charging,
            "SuspendedEVSE" => ChargePointStatus.SuspendedEVSE,
            "SuspendedEV" => ChargePointStatus.SuspendedEV,
            "Finishing" => ChargePointStatus.Finishing,
            "Reserved" => ChargePointStatus.Reserved,
            "Unavailable" => ChargePointStatus.Unavailable,
            "Faulted" => ChargePointStatus.Faulted,
            _ => ChargePointStatus.Unknown
        };
    }
    private string CreateCallError(string messageId, string errorCode, string errorDescription)
    {
        return JsonSerializer.Serialize(new object[]
        {
        4, // MessageType: CallError
        messageId,
        errorCode,
        errorDescription,
        new object() // ErrorDetails (peut être vide)
        });
    }


}
public class OcppRequest
{
    [Required]
    public string ChargePointId { get; set; }

    [Range(1, int.MaxValue)]
    public int ConnectorId { get; set; }
}
public class RemoteStartRequest
{
    [JsonPropertyName("chargePointId")]
    public int? ChargePointId { get; set; } // Notez le ? pour nullable

    [Required]
    [Range(1, int.MaxValue)]
    [JsonPropertyName("connectorId")]
    public int ConnectorId { get; set; }
}