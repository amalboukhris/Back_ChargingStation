using Microsoft.AspNetCore.SignalR;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargingStation.Models
{
    // Cette classe est utilisée pour la persistance dans la base de données
    public class NotificationData
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }
        public bool IsRead { get; set; } = false; // Ajoutez ce champ
        public int ClientId { get; set; }
        public virtual Client Client { get; set; }
    }

    // Cette classe gère les notifications SignalR
    public class NotificationHub : Hub
    {
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        // Modifiez cette méthode pour utiliser ReceiveGlobalNotification
        public async Task SendNotificationToAll(string message)
        {
            await Clients.All.SendAsync("ReceiveGlobalNotification", new
            {
                Message = message,
                Date = DateTime.UtcNow.ToString("g"),
                IsGlobal = true
            });
        }

        // Modifiez cette méthode pour utiliser ReceiveGlobalNotification
        public async Task SendNotificationToUser(string userId, string message)
        {
            await Clients.User(userId).SendAsync("ReceiveGlobalNotification", new
            {
                Message = message,
                Date = DateTime.UtcNow.ToString("g"),
                IsGlobal = false
            });
        }
}

}