using ChargingStation.Models;
using System.ComponentModel.DataAnnotations;

public class Station
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [StringLength(200)]
    public string Address { get; set; }

    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Range(-180, 180)]
    public double Longitude { get; set; }

    // Relations
    public ICollection<ChargePoint> ChargePoints { get; set; } = new List<ChargePoint>();
}