

using System.ComponentModel.DataAnnotations;

namespace ChargingStation.Models
{
    public class ChargePoint
    {
        public int Id { get; set; }
        public int ChargePointId { get; set; } // OCPP ID
        public int StationId { get; set; }
        public ChargePointStatus Status { get; set; }
        public DateTime LastHeartbeat { get; set; }

   
        public Station Station { get; set; }
        public ICollection<Connector> Connectors { get; set; }
        public ICollection<Reservation> Reservations { get; set; }
        public ICollection<Transaction> Transactions { get; set; }
        public string Name { get; internal set; }
        public DateTime CreatedAt { get; internal set; }
       
        public string Model { get; internal set; }
        public string? DiagnosticsStatus { get; internal set; }
        public DateTime LastDiagnosticsTime { get; internal set; }
        public string? FirmwareStatus { get; internal set; }
        public DateTime LastFirmwareUpdate { get; internal set; }
    }
}
