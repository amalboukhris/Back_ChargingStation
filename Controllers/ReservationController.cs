using ChargingStation.Models;
using VehicleChargingStation.Dto;
using System.Linq;

public static class ReservationMapper
{
    public static ReservationDto ToReservationDto(this Reservation reservation)
    {
        if (reservation == null)
            return null;

        return new ReservationDto
        {
            Id = reservation.Id,
            ReservationCode = reservation.ReservationCode,
            ChargePointId = reservation.ChargePointId,
            ChargePointName = reservation.ChargePoint?.Name ?? "N/A",
            StationId = reservation.ChargePoint?.StationId ?? 0,
            StationName = reservation.ChargePoint?.Station?.Name ?? "N/A",
            ConnectorId = reservation.Connector?.ConnectorId ?? 0,
            ConnectorType = reservation.Connector?.Type.ToString() ?? "N/A",
            StartTime = reservation.StartTime,
            EndTime = reservation.EndTime,
            Status = reservation.Status,
            DurationHours = (reservation.EndTime - reservation.StartTime).TotalHours,
            IsActive = reservation.Status == "Active" &&
                      reservation.StartTime <= DateTime.UtcNow &&
                      reservation.EndTime >= DateTime.UtcNow,
            UserInfo = new UserShortInfoDto
            {
                Id = reservation.User?.Id ?? 0,
                FirstName = reservation.User?.FirstName ?? "N/A",
                LastName = reservation.User?.LastName ?? "N/A",
                Email = reservation.User?.Email ?? "N/A"
            },
            StationLocation = new LocationDto
            {
                Latitude = reservation.ChargePoint?.Station?.Latitude ?? 0,
                Longitude = reservation.ChargePoint?.Station?.Longitude ?? 0,
                Address = reservation.ChargePoint?.Station?.Address ?? "N/A"
            },
            CreatedAt = reservation.CreatedAt,
            UpdatedAt = reservation.UpdatedAt
        };
    }

    public static IQueryable<ReservationDto> ToReservationDtoQuery(this IQueryable<Reservation> query)
    {
        return query.Select(r => new ReservationDto
        {
            Id = r.Id,
            ReservationCode = r.ReservationCode,
            ChargePointId = r.ChargePointId,
            ChargePointName = r.ChargePoint.Name,
            StationId = r.ChargePoint.StationId,
            StationName = r.ChargePoint.Station != null ? r.ChargePoint.Station.Name : "N/A",
            ConnectorId = r.Connector.ConnectorId,
            ConnectorType = r.Connector.Type.ToString(),
            StartTime = r.StartTime,
            EndTime = r.EndTime,
            Status = r.Status,
            DurationHours = (r.EndTime - r.StartTime).TotalHours,
            IsActive = r.Status == "Active" &&
                      r.StartTime <= DateTime.UtcNow &&
                      r.EndTime >= DateTime.UtcNow,
            UserInfo = new UserShortInfoDto
            {
                Id = r.User.Id,
                FirstName = r.User.FirstName,
                LastName = r.User.LastName,
                Email = r.User.Email
            },
            StationLocation = new LocationDto
            {
                Latitude = r.ChargePoint.Station != null ? r.ChargePoint.Station.Latitude : 0,
                Longitude = r.ChargePoint.Station != null ? r.ChargePoint.Station.Longitude : 0,
                Address = r.ChargePoint.Station != null ? r.ChargePoint.Station.Address : "N/A"
            },
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        });
    }
}