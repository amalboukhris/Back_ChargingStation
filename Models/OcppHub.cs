//using ChargingStation.Data;
//using ChargingStation.Models;
//using Microsoft.AspNetCore.SignalR;
//using Microsoft.EntityFrameworkCore;
//using Newtonsoft.Json.Linq;
//using System.Text;

//namespace ChargingStation.Hubs
//{
//    public class OcppHub : Hub
//    {
//        private readonly AppDbContext _context;
//        private readonly ILogger<OcppHub> _logger;
//        private readonly IOcpp _ocppService;

//        public OcppHub(AppDbContext context, ILogger<OcppHub> logger, IOcpp ocppService)
//        {
//            _context = context;
//            _logger = logger;
//            _ocppService = ocppService;
//        }

//        public override async Task OnConnectedAsync()
//        {
//            var chargePointId = Context.GetHttpContext()?.Request.Query["chargePointId"].FirstOrDefault();
//            if (!string.IsNullOrEmpty(chargePointId))
//            {
//                _logger.LogInformation($"ChargePoint {chargePointId} connected (ConnectionId: {Context.ConnectionId})");
//                await Groups.AddToGroupAsync(Context.ConnectionId, chargePointId);

//                // Update charge point status
//                var cp = await _context.ChargePoints.FirstOrDefaultAsync(c => c.ChargePointId == chargePointId);
//                if (cp != null)
//                {
//                    cp.Status = ChargePointStatus.Available;

//                    cp.LastHeartbeat = DateTime.UtcNow;
//                    await _context.SaveChangesAsync();
//                }
//            }
//            await base.OnConnectedAsync();
//        }

//        public override async Task OnDisconnectedAsync(Exception? exception)
//        {
//            var chargePointId = Context.GetHttpContext()?.Request.Query["chargePointId"].FirstOrDefault();
//            if (!string.IsNullOrEmpty(chargePointId))
//            {
//                _logger.LogInformation($"ChargePoint {chargePointId} disconnected");
//                await Groups.RemoveFromGroupAsync(Context.ConnectionId, chargePointId);

//                // Update charge point status
//                var cp = await _context.ChargePoints.FirstOrDefaultAsync(c => c.ChargePointId == chargePointId);
//                if (cp != null)
//                {
//                    cp.Status = ChargePointStatus.Unavailable;
//                    await _context.SaveChangesAsync();
//                }
//            }
//            await base.OnDisconnectedAsync(exception);
//        }

//        public async Task ProcessOcppMessage(string chargePointId, string message)
//        {
//            try
//            {
//                var ocppMessage = JArray.Parse(message);
//                if (ocppMessage.Count < 4)
//                {
//                    _logger.LogWarning($"Invalid OCPP message format from {chargePointId}");
//                    return;
//                }

//                var messageType = (int)ocppMessage[0];
//                var uniqueId = (string)ocppMessage[1];
//                var action = (string)ocppMessage[2];
//                var payload = (JObject)ocppMessage[3];

//                _logger.LogInformation($"Processing {action} from {chargePointId}");

//                switch (action)
//                {
//                    case "BootNotification":
//                        await HandleBootNotification(chargePointId, uniqueId, payload);
//                        break;
//                    case "Authorize":
//                        await HandleAuthorize(chargePointId, uniqueId, payload);
//                        break;
//                    case "StartTransaction":
//                        await HandleStartTransaction(chargePointId, uniqueId, payload);
//                        break;
//                    case "StopTransaction":
//                        await HandleStopTransaction(chargePointId, uniqueId, payload);
//                        break;
//                    case "Heartbeat":
//                        await HandleHeartbeat(chargePointId, uniqueId);
//                        break;
//                    case "StatusNotification":
//                        await HandleStatusNotification(chargePointId, payload);
//                        break;
//                    case "MeterValues":
//                        await HandleMeterValues(chargePointId, payload);
//                        break;
//                    case "ReserveNow":
//                        await HandleReserveNow(chargePointId, uniqueId, payload);
//                        break;
//                    default:
//                        _logger.LogWarning($"Unsupported OCPP action: {action}");
//                        break;
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Error processing OCPP message from {chargePointId}");
//            }
//        }

//        private async Task HandleReserveNow(string chargePointId, string uniqueId, JObject payload)
//        {
//            var response = new JArray
//            {
//                3, // CALLRESULT
//                uniqueId,
//                new JObject
//                {
//                    ["status"] = "Accepted"
//                }
//            };

//            await Clients.Caller.SendAsync("SendOcppResponse", response.ToString());
//        }

//        private async Task HandleBootNotification(string chargePointId, string uniqueId, JObject payload)
//        {
//            var chargePoint = await _context.ChargePoints
//                .FirstOrDefaultAsync(cp => cp.ChargePointId == chargePointId);

//            if (chargePoint == null)
//            {
//                chargePoint = new ChargePoint
//                {
//                    ChargePointId = chargePointId,
//                    Status = ChargePointStatus.Available,
//                    LastHeartbeat = DateTime.UtcNow
//                };
//                _context.ChargePoints.Add(chargePoint);
//            }



//            await _context.SaveChangesAsync();

//            var response = new JArray
//            {
//                3, // CALLRESULT
//                uniqueId,
//                new JObject
//                {
//                    ["status"] = "Accepted",
//                    ["currentTime"] = DateTime.UtcNow.ToString("o"),
//                    ["interval"] = 300 // Intervalle de heartbeat en secondes
//                }
//            };

//            await Clients.Caller.SendAsync("SendOcppResponse", response.ToString());
//        }

//        private async Task HandleAuthorize(string chargePointId, string uniqueId, JObject payload)
//        {
//            var idTag = payload["idTag"]?.ToString();
//            var response = new JArray
//            {
//                3, // CALLRESULT
//                uniqueId,
//                new JObject
//                {
//                    ["idTagInfo"] = new JObject
//                    {
//                        ["status"] = "Accepted",
//                        ["expiryDate"] = DateTime.UtcNow.AddDays(1).ToString("o"),
//                        ["parentIdTag"] = idTag
//                    }
//                }
//            };

//            await Clients.Caller.SendAsync("SendOcppResponse", response.ToString());
//        }

//        private async Task HandleStartTransaction(string chargePointId, string uniqueId, JObject payload)
//        {
//            var connectorId = (int)payload["connectorId"];
//            var idTag = payload["idTag"]?.ToString();
//            var meterStart = (int)payload["meterStart"];
//            var materStop = DateTime.Parse(payload["timestamp"]?.ToString());

//            var transaction = new Transaction
//            {
//                ChargePointId = _context.ChargePoints.First(cp => cp.ChargePointId == chargePointId).Id,
//                ConnectorId = connectorId,
//                IdTag = idTag,
//                MeterStart = meterStart,

//                Status = "Active"
//            };

//            _context.Transactions.Add(transaction);
//            await _context.SaveChangesAsync();

//            var response = new JArray
//            {
//                3, // CALLRESULT
//                uniqueId,
//                new JObject
//                {
//                    ["transactionId"] = transaction.Id,
//                    ["idTagInfo"] = new JObject
//                    {
//                        ["status"] = "Accepted"
//                    }
//                }
//            };

//            await Clients.Caller.SendAsync("SendOcppResponse", response.ToString());
//        }

//        private async Task HandleStopTransaction(string chargePointId, string uniqueId, JObject payload)
//        {
//            var transactionId = (int)payload["transactionId"];
//            var meterStop = (int)payload["meterStop"];
//            var timestamp = DateTime.Parse(payload["timestamp"]?.ToString());
//            var reason = payload["reason"]?.ToString();

//            var transaction = await _context.Transactions.FindAsync(transactionId);
//            if (transaction != null)
//            {
//                transaction.MeterStop = meterStop;
//                transaction.StopTimestamp = timestamp;
//                transaction.Status = "Completed";
//                transaction.Reason = reason;

//                await _context.SaveChangesAsync();
//            }

//            var response = new JArray
//            {
//                3, // CALLRESULT
//                uniqueId,
//                new JObject
//                {
//                    ["idTagInfo"] = new JObject
//                    {
//                        ["status"] = "Accepted"
//                    }
//                }
//            };

//            await Clients.Caller.SendAsync("SendOcppResponse", response.ToString());
//        }

//        private async Task HandleHeartbeat(string chargePointId, string uniqueId)
//        {
//            var chargePoint = await _context.ChargePoints
//                .FirstOrDefaultAsync(cp => cp.ChargePointId == chargePointId);

//            if (chargePoint != null)
//            {
//                chargePoint.LastHeartbeat = DateTime.UtcNow;
//                await _context.SaveChangesAsync();
//            }

//            var response = new JArray
//            {
//                3, // CALLRESULT
//                uniqueId,
//                new JObject
//                {
//                    ["currentTime"] = DateTime.UtcNow.ToString("o")
//                }
//            };

//            await Clients.Caller.SendAsync("SendOcppResponse", response.ToString());
//        }

//        private async Task HandleStatusNotification(string chargePointId, JObject payload)
//        {
//            var connectorId = (int)payload["connectorId"];
//            var status = payload["status"]?.ToString();
//            var errorCode = payload["errorCode"]?.ToString();

//            var chargePoint = await _context.ChargePoints
//                .FirstOrDefaultAsync(cp => cp.ChargePointId == chargePointId);

//            if (chargePoint == null) return;

//            var connector = await _context.Connectors
//                .FirstOrDefaultAsync(c => c.ChargePointId == chargePoint.Id && c.ConnectorId == connectorId);

//            if (connector == null)
//            {
//                connector = new Connector
//                {
//                    ChargePointId = chargePoint.Id,
//                    ConnectorId = connectorId,
//                    Status = Enum.Parse<ChargePointStatus>(status)
//                };
//                _context.Connectors.Add(connector);
//            }
//            else
//            {
//                connector.Status = Enum.Parse<ChargePointStatus>(status);
//            }

//            await _context.SaveChangesAsync();
//        }

//        private async Task HandleMeterValues(string chargePointId, JObject payload)
//        {
//            var connectorId = (int)payload["connectorId"];
//            var transactionId = payload["transactionId"]?.ToObject<int?>();
//            var meterValue = payload["meterValue"]?.First?["sampledValue"]?.First?["value"]?.ToString();

//            if (double.TryParse(meterValue, out var value))
//            {
//                var chargePoint = await _context.ChargePoints
//                    .FirstOrDefaultAsync(cp => cp.ChargePointId == chargePointId);

//                if (chargePoint == null) return;

//                var connector = await _context.Connectors
//                    .FirstOrDefaultAsync(c => c.ChargePointId == chargePoint.Id && c.ConnectorId == connectorId);

//                if (connector != null)
//                {
//                    connector.MeterValue = value;
//                    connector.MeterValueTimestamp = DateTime.UtcNow;
//                    connector.TransactionId = transactionId;

//                    await _context.SaveChangesAsync();
//                }
//            }
//        }
//    }
//}

