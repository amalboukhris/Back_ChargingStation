//using Microsoft.AspNetCore.Mvc;
//using VehicleChargingStation.serveur;
//using System.Linq;
//using System.Threading.Tasks;

//namespace VehicleChargingStation.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class OcppController : ControllerBase
//    {
//        private readonly OcppWebSocketService _ocppService;

//        public OcppController(OcppWebSocketService ocppService)
//        {
//            _ocppService = ocppService;
//        }

//        [HttpPost("sendCommand")]
//        public async Task<IActionResult> SendCommand([FromBody] string command)
//        {
//            var clients = _ocppService.GetConnectedClients().ToList();

//            if (!clients.Any())
//                return BadRequest("Aucune borne connectée");

//            await _ocppService.SendToAll(command);

//            return Ok("Commande envoyée à toutes les bornes connectées");
//        }
//    }
//}
