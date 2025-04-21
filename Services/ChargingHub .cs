using Microsoft.AspNetCore.SignalR;

namespace ChargingStation.Hubs
{
    public interface IChargingHubClient
    {
        Task ConnectorStatusChanged(object status);
    }

    public class ChargingHub : Hub<IChargingHubClient>
    {
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }
    }

}