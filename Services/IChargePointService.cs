
using ChargingStation.Models;
using ChargingStation.Models.ChargingStation.Models.Ocpp;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChargingStation.Services
{
    public interface IChargePointService
    {
        // Gestion des bornes
        Task<ChargePoint> RegisterChargePointAsync(ChargePointCreateDto dto, CancellationToken cancellationToken = default);
        Task<ChargePoint> GetChargePointByIdAsync(int id);
        Task<ChargePoint> GetChargePointByOcppIdAsync(int ocppId, CancellationToken cancellationToken = default);
        Task<IEnumerable<ChargePoint>> GetChargePointsAsync(ChargePointFilterDto filter);
        Task UpdateChargePointStatusAsync(int ocppId, ChargePointStatus status, CancellationToken cancellationToken = default);

        // Gestion des connecteurs
        Task<Connector> AddConnectorAsync(int chargePointId, ConnectorCreateDto dto);
        Task UpdateConnectorStatusAsync(int chargePointId, int connectorId, ConnectorStatus status);

        // Réservations
        Task<Reservation> CreateReservationAsync(ReservationCreateDto dto);
        Task CancelReservationAsync(int reservationId, string cancellationReason);

        // Transactions
        Task<Transaction> StartTransactionAsync(TransactionStartDto dto);
        Task<Transaction> StopTransactionAsync(TransactionStopDto dto);

        // OCPP
        Task ProcessOcppMessageAsync(int chargePointId, OcppMessage message);
        Task SendRemoteCommandAsync(int chargePointId, OcppCommand command);

        // Monitoring
        Task<ChargePointStatusDto> GetChargePointStatusAsync(string ocppId);
        Task<IEnumerable<ChargePointStatusDto>> GetChargePointsStatusAsync(ChargePointStatusFilter filter);
        Task<ChargePoint> RegisterConnectionAsync(int chargePointId);
        Task MarkAsDisconnectedAsync(int chargePointId);
      
    }

    public class ChargePointCreateDto
    {
        public string Name { get; internal set; }
        public int OcppId { get; internal set; }
        public int StationId { get; internal set; }
    }

    public class ConnectorCreateDto
    {
        public int ConnectorId { get; internal set; }
        
        public double MaxPower { get; internal set; }
        public string ConnectorType { get; internal set; }
    }

    public class ReservationCreateDto
    {
        public int ChargePointId { get; internal set; }
        public DateTime StartTime { get; internal set; }
        public DateTime EndTime { get; internal set; }
        public int UserId { get; internal set; }
        public int ConnectorId { get; internal set; }
    }

    public class TransactionStartDto
    {
        public int ConnectorId { get; internal set; }
        public string IdTag { get; internal set; }
        public int MeterStart { get; internal set; }
        public int OcppId { get; internal set; }
    }

    public class TransactionStopDto
    {
        public int TransactionId { get; internal set; }
        public int? MeterStop { get; internal set; }
    }

    public class OcppCommand
    {
        public object Action { get; internal set; }
    }

    public class ChargePointFilterDto
    {
        public int? StationId { get; set; }
        public ChargePointStatus? Status { get; set; }
        public DateTime? LastHeartbeatFrom { get; set; }
        public DateTime? LastHeartbeatTo { get; set; }
        public int OcppId { get; internal set; }
    }

    public class ChargePointStatusFilter
    {
   
       
        public bool? IncludeConnectors { get; set; } = false;
        public int OcppId { get; internal set; }
        public ChargePointStatus Status { get; set; }
        public object StationId { get; internal set; }
    }

    public class ChargePointStatusDto
    {
        public int OcppId { get; set; }
        public string Name { get; set; }
        public ChargePointStatus Status { get; set; }
        public DateTime LastHeartbeat { get; set; }
        public IEnumerable<ConnectorStatusDto> Connectors { get; set; }
    }

    public class ConnectorStatusDto
    {
        public int ConnectorId { get; set; }
        public ConnectorStatus Status { get; set; }
        public double? CurrentPower { get; set; } // kW
        public int Id { get; internal set; }
        public double MaxPower { get; internal set; }
        public DateTime LastUpdated { get; internal set; }
        public string ChargePointId { get; internal set; }
        public DateTime Timestamp { get; internal set; }
    }

    public enum ConnectorStatus
    {
        Available,
        Occupied,
        Faulted,
        Unavailable,
        Reserved
    }
}