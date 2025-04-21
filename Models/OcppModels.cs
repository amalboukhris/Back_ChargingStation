// Models/OcppModels.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChargingStation.Models
{
    public enum OcppVersion
    {
        OCPP16,
        OCPP20
    }

    public enum ChargePointStatus
    {
        Available,
        Preparing,
        Charging,
        SuspendedEVSE,
        SuspendedEV,
        Finishing,
        Reserved,
        Unavailable,
        Faulted,
        Unknown,
        Offline,
        Online,
        Value,
    }





    namespace ChargingStation.Models.Ocpp
    {
        public class OcppMessage
        {
            [JsonPropertyName("messageType")]
            public string MessageType { get; set; }

            [JsonPropertyName("payload")]
            public JsonElement Payload { get; set; }
            public string Action { get; set; }
        }

        public class OcppResponse
        {
            [JsonPropertyName("messageType")]
            public string MessageType { get; set; }

            [JsonPropertyName("payload")]
            public object Payload { get; set; }
        }

        public class StatusNotificationPayload
        {
            [JsonPropertyName("connectorId")]
            public int ConnectorId { get; set; }

            [JsonPropertyName("status")]
            public string Status { get; set; }

            [JsonPropertyName("timestamp")]
            public DateTime Timestamp { get; set; }
        }

        public class StartTransactionPayload
        {
            [JsonPropertyName("connectorId")]
            public int ConnectorId { get; set; }

            [JsonPropertyName("idTag")]
            public string IdTag { get; set; }

            [JsonPropertyName("meterStart")]
            public int MeterStart { get; set; }

            [JsonPropertyName("timestamp")]
            public DateTime Timestamp { get; set; }
        }
    }
}