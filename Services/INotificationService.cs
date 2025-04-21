namespace ChargingStation.Services
{
    // INotificationService.cs
    // INotificationService.cs
    public interface INotificationService
    {
        Task SendAsync(string deviceToken, string title, string message);
        // Add other notification methods as needed
    }
}
