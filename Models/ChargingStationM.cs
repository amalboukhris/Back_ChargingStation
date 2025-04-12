using System.ComponentModel.DataAnnotations;


namespace ChargingStation.Models
{
    public class ChargingStationM
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)] 
        public string Name { get; set; }

        [Required]
        public double Latitude { get; set; } 

        [Required]
        public double Longitude { get; set; }

        public string? Description { get; set; } 

        public bool Availability { get; set; } = true; 

        public virtual ICollection<Borne> Bornes { get; set; } = new HashSet<Borne>();
       
    }


}