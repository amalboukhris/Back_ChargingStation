using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ChargingStation.Models
{
    public class Borne
    {
        public int Id { get; set; }
        public int ChargingStationId { get; set; }
        public int? ReservationUserId { get; set; }
      
        public string Nom { get; set; }
        public string Etat { get; set; } = "Disponible";

        [JsonIgnore]
        public virtual ChargingStationM ChargingStation { get; set; }
       
        //public string Reservations { get; internal set; }
    }

}