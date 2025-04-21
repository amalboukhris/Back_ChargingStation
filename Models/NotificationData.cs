//using Microsoft.AspNetCore.SignalR;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace ChargingStation.Models
//{

//    public class NotificationData
//    {
//        public int Id { get; set; }
//        public string Message { get; set; }
//        public DateTime Date { get; set; }
//        public bool IsRead { get; set; } = false; // Ajoutez ce champ
//        public int ClientId { get; set; }
//        public virtual Client Client { get; set; }
//        public bool IsGlobal { get; internal set; }
//        public bool IsDeleted { get; set; } = false;
//    }

//    public class NotificationHub : Hub
//    {
//        public async Task JoinGroup(string groupName)
//        {
//            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
//        }

//        public async Task LeaveGroup(string groupName)
//        {
//            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
//        }


//        public async Task SendNotificationToAll(string message)
//        {
//            await Clients.All.SendAsync("ReceiveGlobalNotification", new
//            {
//                Message = message,
//                Date = DateTime.UtcNow.ToString("g"),
//                IsGlobal = true
//            });
//        }


//        public async Task SendNotificationToUser(string userId, string message)
//        {
//            await Clients.User(userId).SendAsync("ReceiveGlobalNotification", new
//            {
//                Message = message,
//                Date = DateTime.UtcNow.ToString("g"),
//                IsGlobal = false
//            });
//        }
//}

//}
using Microsoft.AspNetCore.SignalR;

public class NotificationHub : Hub
{
    public async Task SubscribeToChargePoint(string chargePointId)
{
    await Groups.AddToGroupAsync(Context.ConnectionId, chargePointId);
}
}

