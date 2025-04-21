// FirmwareService.cs
using ChargingStation.Data;

namespace ChargingStation.Services
{
    public class FirmwareService : IFirmwareService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<FirmwareService> _logger;

        public FirmwareService(AppDbContext context, ILogger<FirmwareService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task UpdateFirmwareStatusAsync(int chargePointId, string status)
        {
            var chargePoint = await _context.ChargePoints.FindAsync(chargePointId);
            if (chargePoint != null)
            {
                chargePoint.FirmwareStatus = status;
                chargePoint.LastFirmwareUpdate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Updated firmware status for charge point {chargePointId} to {status}");
            }
        }

        public Task UpdateStatusAsync(int chargePointId, string status)
        {
            throw new NotImplementedException();
        }
    }
}