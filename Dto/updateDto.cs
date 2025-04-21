
using ChargingStation.Models;

namespace VehicleChargingStation.Dto
{
    // DTOs/StationDto.cs
    public class StationDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int AvailableChargePoints { get; set; }
        public int TotalChargePoints { get; set; }
    }

    // DTOs/ChargePointDto.cs
    public class ChargePointDto
    {
        public int Id { get; set; }
        public int ChargePointId { get; set; }
        public string Name { get; set; }
        public ChargePointStatus Status { get; set; }
      
        public string StationName { get; internal set; }
        public string Model { get; internal set; }
       
        internal List<ConnectorDto> Connectors { get; set; }
    }

    // DTOs/ReservationDto.cs
    public class ReservationDto
    {
        public int Id { get; set; }
        public int ChargePointId { get; set; }
        public int ConnectorId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; }
        public string ReservationCode { get; set; }
    }

    
        public class ReservationNotificationDto
        {
            public int ReservationId { get; set; }
            public string ReservationCode { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public string Status { get; set; }

            // Charge Point Info
            public int ChargePointId { get; set; }
            public string ChargePointName { get; set; }
            public string StationName { get; set; }

            // Connector Info
            public int ConnectorId { get; set; }
            public string ConnectorType { get; set; }
            public double MaxPower { get; set; } // kW

            // User Info
            public string UserId { get; set; }
            public string UserFirstName { get; set; }
            public string UserLastName { get; set; }

            // Timestamps
            public DateTime CreatedAt { get; set; }
            public DateTime? CancelledAt { get; set; }

            // Calculated fields
            public double DurationHours => (EndTime - StartTime).TotalHours;
            public bool IsActive => Status == "Active" && StartTime <= DateTime.UtcNow && EndTime >= DateTime.UtcNow;

            public ReservationNotificationDto() { }

            public ReservationNotificationDto(Reservation reservation)
            {
                ReservationId = reservation.Id;
                ReservationCode = reservation.ReservationCode;
                StartTime = reservation.StartTime;
                EndTime = reservation.EndTime;
                Status = reservation.Status;

                ChargePointId = reservation.ChargePoint.Id;
                ChargePointName = reservation.ChargePoint.Name;
                StationName = reservation.ChargePoint.Station?.Name ?? "Unknown Station";

                ConnectorId = reservation.Connector.ConnectorId;
                ConnectorType = reservation.Connector.Type.ToString();
                MaxPower = reservation.Connector.MaxPower;

                //UserId = reservation.UserId;
                UserFirstName = reservation.User.FirstName;
                UserLastName = reservation.User.LastName;

                CreatedAt = reservation.CreatedAt;
                CancelledAt = reservation.CancelledAt;
            }
        }
    }
public class ReservationDto
{


    public int Id { get; set; }

    // Charge Point Info
    public int ChargePointId { get; set; }
    public string ChargePointName { get; set; }
    public string StationName { get; set; }

    // Connector Info
    public int ConnectorId { get; set; }
    public string ConnectorType { get; set; }

    // Time Info
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public double DurationHours { get; set; }

    // Status
    public string Status { get; set; }
    public string ReservationCode { get; set; }
    public bool IsActive { get; set; }

    // User Info
    public UserShortInfoDto UserInfo { get; set; }

    // Location
    public LocationDto StationLocation { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int UserId { get; internal set; }
    public int StationId { get; internal set; }
}

public class UserShortInfoDto
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
}

public class LocationDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Address { get; set; }
}

public class ConnectorStatusDto
{
    public int ChargePointId { get; set; }
    public int ConnectorId { get; set; }
    public string Status { get; set; }
    public DateTime Timestamp { get; set; }
}

public class ChargePointCreateDto
{
    public string OcppId { get; set; }
    public string Name { get; set; }
    public int? StationId { get; set; }
}

public class ConnectorCreateDto
{
    public int ConnectorId { get; set; }
    public string ConnectorType { get; set; }
    public double MaxPower { get; set; }
}
