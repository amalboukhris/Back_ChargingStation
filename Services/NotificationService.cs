using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ChargingStation.Services;
using Microsoft.Extensions.Configuration;

public class NotificationService: INotificationService
{
    private readonly ILogger<NotificationService> _logger;

    private readonly HttpClient _httpClient;
    private readonly string _fcmServerKey;
    private readonly string _fcmUrl = "https://fcm.googleapis.com/fcm/send";

    public NotificationService(IConfiguration config, ILogger<NotificationService> logger)
    {
        _httpClient = new HttpClient();
        _fcmServerKey = config["Fcm:ServerKey"];
        _logger = logger;// Stockez dans appsettings.json ou User Secrets
    }

    public async Task SendAsync(string deviceToken, string title, string message)
    {
        try
        {
            // Implement actual FCM notification logic here
            _logger.LogInformation($"Sending notification to {deviceToken}: {title} - {message}");

            // Example FCM implementation would go here
            // await _fcmClient.SendAsync(deviceToken, title, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification");
            throw;
        }
    }

    public async Task SendNotificationAsync(string deviceToken, string title, string body)
    {
        var message = new
        {
            to = deviceToken,
            notification = new
            {
                title = title,
                body = body
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, _fcmUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("key", "=" + _fcmServerKey);
        request.Content = new StringContent(JsonSerializer.Serialize(message), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public Task SendNotificationAsync(string message)
    {
        throw new NotImplementedException();
    }
}
