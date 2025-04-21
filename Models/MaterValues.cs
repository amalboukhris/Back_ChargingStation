namespace ChargingStation.Models
{
    public class MeterValue
    {
        public int Id { get; set; }

        public int ChargePointId { get; set; }
        public int ConnectorId { get; set; }
        public int TransactionId { get; set; }

        public DateTime Timestamp { get; set; }
        public decimal Value { get; set; }

        public string Measurand { get; set; }
        public string Unit { get; set; }

        // Navigation properties si nécessaire
        public ChargePoint ChargePoint { get; set; }
    }

}
