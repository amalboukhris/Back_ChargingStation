using Umbraco.Core.Models;

namespace ChargingStation.Models
{
    public class Client : User
    {
        public int Id { get; set; }



        public virtual ICollection<NotificationData> Notifications { get; set; } = new HashSet<NotificationData>();
    }
}
