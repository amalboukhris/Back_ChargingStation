// Dans ChargingStation.Models
using ChargingStation.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public enum ConnectorStatus
{
    Available,
    Occupied,
    Reserved,
    Unavailable,
    Faulted
}

public class Connector
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int Id { get; set; }

    public int ConnectorId { get; set; }

    public int ChargePointId { get; set; }
    public string Type { get; set; }
    public ChargePointStatus Status { get; set; }
    public double MaxPower { get; set; }
    public DateTime LastUpdated { get; set; }
    [ForeignKey("ChargePointId")]
    public ChargePoint ChargePoint { get; set; }
    public ICollection<Transaction> Transactions { get; set; }
    public ICollection<Reservation> Reservations { get; set; }
}