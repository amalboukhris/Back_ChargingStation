
using ChargingStation.Data;
using ChargingStation.Hubs;
using ChargingStation.Models.ChargingStation.Models.Ocpp;
using ChargingStation.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChargingStation.Services
{
    public interface IOcppProtocolService
    {
        Task<bool> RemoteStartTransaction(int chargePointId, int connectorId);
        Task ProcessMessageAsync(int chargePointId, OcppMessage message);
        Task SendCommandAsync(int chargePointId, OcppCommand command);
        Task<bool> SendRemoteStartTransactionAsync(int chargePointId, int connectorId, string idTag);
        Task<bool> SendRemoteStopTransactionAsync(int chargePointId, int transactionId);

    }
}
public class OcppProtocolService : IOcppProtocolService
{
    private readonly ILogger<OcppProtocolService> _logger;
    private readonly AppDbContext _context;
    private readonly IHubContext<ChargingHub> _hubContext;

    public OcppProtocolService(
        ILogger<OcppProtocolService> logger,
        AppDbContext context,
        IHubContext<ChargingHub> hubContext)
    {
        _logger = logger;
        _context = context;
        _hubContext = hubContext;
    }

    public async Task<bool> RemoteStartTransaction(int chargePointId, int connectorId)
    {
        var connector = await _context.Connectors
            .FirstOrDefaultAsync(c => c.ChargePointId == chargePointId && c.ConnectorId == connectorId);

        if (connector == null)
        {
            _logger.LogWarning($"Connector {connectorId} not found for charge point {chargePointId}");
            return false;
        }

        // Logique de démarrage de transaction
        return true;
    }

    public Task ProcessMessageAsync(int chargePointId, OcppMessage message)
    {
        _logger.LogInformation($"Processing message for {chargePointId}: {message.Action}");
        return Task.CompletedTask;
    }

    public Task SendCommandAsync(int chargePointId, OcppCommand command)
    {
        _logger.LogInformation($"Sending command to {chargePointId}: {command.Action}");
        return Task.CompletedTask;
    }

    public async Task<bool> SendRemoteStartTransactionAsync(int chargePointId, int connectorId, string idTag)
    {
        try
        {
            await _hubContext.Clients.Group(chargePointId.ToString())
                .SendAsync("RemoteStartTransaction", new
                {
                    ConnectorId = connectorId,
                    IdTag = idTag
                });
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send remote start to {chargePointId}");
            return false;
        }
    }

    public async Task<bool> SendRemoteStopTransactionAsync(int chargePointId, int transactionId)
    {
        try
        {
            await _hubContext.Clients.Group(chargePointId.ToString())
                .SendAsync("RemoteStopTransaction", transactionId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send remote stop to {chargePointId}");
            return false;
        }
    }

}