//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace ChargingStation.Models
//{
//    public class Reservation
//    {
//        public int Id { get; set; }

//        [Required]
//        public int BorneId { get; set; }

//        [ForeignKey("BorneId")]
//        public virtual Borne Borne { get; set; }

//        [Required]
//        public DateTime DateDebut { get; set; }

//        [Required]
//        public DateTime DateFin { get; set; }

//        public string? ClientId { get; set; } // ou autre info d'utilisateur
       
//    }
//}
