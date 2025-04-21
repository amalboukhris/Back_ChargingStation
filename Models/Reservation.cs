using ChargingStation.Models;
using System.ComponentModel.DataAnnotations.Schema;
[Table("Reservations")]
public class Reservation
{
    public int Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = "Active"; // Active, Completed, Cancelled

    // Attributs supplémentaires
    public string? ReservationCode { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
  

    // Relations
    public int ChargePointId { get; set; }
    public ChargePoint ChargePoint { get; set; }

    public int ConnectorId { get; set; }
    public Connector Connector { get; set; }

    public int UserId { get; set; }
    public Client User { get; set; }

    public DateTime? UpdatedAt { get; internal set; }
    public ICollection<Transaction> Transactions { get; set; }

    
}
public enum ReservationStatus
{
    Active,
    Cancelled,
    Completed
}