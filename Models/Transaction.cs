using ChargingStation.Models;
using System.ComponentModel.DataAnnotations;

public class Transaction
{
    public int Id { get; set; }
    public int ChargePointId { get; set; }
    public int ConnectorId { get; set; }

    [StringLength(50)]
    public string IdTag { get; set; } // RFID ou ID utilisateur

    public int MeterStart { get; set; }
    public int? MeterStop { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    [StringLength(20)]
    public string Status { get; set; } // Active, Completed, Cancelled

    // Relations
    public ChargePoint ChargePoint { get; set; }
    public Connector Connector { get; set; }
    public int? ReservationId { get; set; }
    public Reservation Reservation { get; set; }
    public DateTime? StopTimestamp { get; set; }
    public string? Reason { get; set; }

}