using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ChargingStation.Data;
using ChargingStation.Models;

namespace ChargingStation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChargingStationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ChargingStationController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
            public async Task<IActionResult> GetAllStations()
            {
                var stations = await _context.ChargingStations
                    .Include(s => s.Bornes)
                    .ToListAsync();

                return Ok(stations);
            }

        // Ajouter une station
        [HttpPost]
        public async Task<IActionResult> CreateStation([FromBody] ChargingStationDto stationDto)
        {
            if (stationDto == null)
                return BadRequest("Données invalides.");

            var station = new ChargingStationM
            {
                Name = stationDto.Name,
                Latitude = stationDto.Latitude,
                Longitude = stationDto.Longitude,
                Availability = stationDto.Availability
            };

            _context.ChargingStations.Add(station);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAllStations), new { id = station.Id }, station);
        }

        // GET: api/ChargingStation/search?name=NomDeLaStation
        [HttpGet("search")]
        public async Task<IActionResult> SearchStations([FromQuery] string name)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                    return BadRequest("Le nom de la station est requis pour la recherche.");

                var stations = await _context.ChargingStations
                    .Include(s => s.Bornes)
                    .Where(s => s.Name.ToLower().Contains(name.ToLower()))
                    .ToListAsync();

                if (!stations.Any())
                    return NotFound($"Aucune station trouvée avec le nom contenant \"{name}\".");

                return Ok(stations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne du serveur : {ex.Message}");
            }
        }


    }


    public class ChargingStationDto
        {
            public int Id { get; set; }

        public string Name { get; set; }

            public double Latitude { get; set; }

            public double Longitude { get; set; }

            public string? Description { get; set; }

            public bool Availability { get; set; } = true;

        }
    }
