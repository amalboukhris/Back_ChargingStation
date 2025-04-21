namespace ChargingStation.Services
{
    public interface IFirmwareService
    {
        Task UpdateFirmwareStatusAsync(int id, string? status);
        Task UpdateStatusAsync(int chargePointId, string status);
    }

    
}
