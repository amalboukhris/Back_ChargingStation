using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

using System.Text;

using ChargingStation.Data;
using ChargingStation.Models;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Umbraco.Core.Persistence.Repositories;
using Microsoft.AspNetCore.JsonPatch;
using VehicleChargingStation.Dto;

namespace VehicleChargingStation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public UserController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
         
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                return BadRequest("Email is already taken.");
            }

           
            user.Password = HashPassword(user.Password);

        
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully" });
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginRequest.Email);
            if (user == null || !VerifyPassword(user.Password, loginRequest.Password))
            {
                return Unauthorized("Invalid email or password.");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(100),
                signingCredentials: creds
            );

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new { Message = "Login successful", Token = jwtToken });
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {

                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }
   
        private bool VerifyPassword(string hashedPassword, string password)
        {
            
            string hashedInputPassword = HashPassword(password);
            return hashedInputPassword == hashedPassword;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return Ok(new
            {
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.PhoneNumber,
                user.DateOfBirth
            });
        }
      
            [HttpGet("profile")]
            [Authorize]
            public async Task<IActionResult> GetProfile()
            {
                try
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                    if (!int.TryParse(userId, out int id))
                    {
                        return BadRequest("Invalid user ID format");
                    }

                    var user = await _context.Users.FindAsync(id);

                    if (user == null)
                    {
                        return NotFound("User not found");
                    }

                    return Ok(new
                    {
                        user.FirstName,
                        user.LastName,
                        user.Email,
                        user.PhoneNumber,
                        DateOfBirth = user.DateOfBirth.ToString("yyyy-MM-dd") // Formatage optionnel
                    });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] updateDto user, [FromHeader] string authorization)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Vérifier le token d'authentification
            var token = authorization?.Split(" ").Last();
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Token is required");
            }

            var userId = ValidateTokenAndGetUserId(token);
            if (userId == null)
            {
                return Unauthorized("Invalid or expired token");
            }

            var existingUser = await _context.Users.FindAsync(userId);
            if (existingUser == null)
            {
                return NotFound("User not found");
            }

            // Mise à jour des informations utilisateur sans toucher au mot de passe
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.Email = user.Email;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.DateOfBirth = user.DateOfBirth;

            // Sauvegarder les modifications dans la base de données
            await _context.SaveChangesAsync();

            // Retourner l'utilisateur mis à jour
            return Ok(existingUser);
        }

        private int? ValidateTokenAndGetUserId(string token)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            try
            {
                // Clé secrète ou clé publique utilisée pour signer et valider le token
                var key = Encoding.ASCII.GetBytes("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjI4IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6ImFtYWxib3VraHJpczFAZ21haWwuY29tIiwiZXhwIjoxNzQzNjgxODAwLCJpc3MiOiJodHRwczovL2xvY2FsaG9zdDo3MjIxIiwiYXVkIjoiaHR0cHM6Ly9sb2NhbGhvc3Q6NzIyMSJ9.XiurKvKsg1dtjTrICy24sDMDHdYvdQsmmxIVsnsf8ZY"); // Remplacez cela par votre clé secrète ou publique

                // Valider le token et extraire les informations de manière sécurisée
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = "votre_issuer", // Remplacez par votre issuer
                    ValidAudience = "votre_audience", // Remplacez par votre audience
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero // Optionnel: définit le délai de grâce pour la validation
                };

                // Valider le token et obtenir les données de l'utilisateur
                var principal = jwtHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);

                // Extraire l'ID de l'utilisateur à partir des claims du token
                var userIdClaim = principal?.Claims.FirstOrDefault(c => c.Type == "userId");
                return userIdClaim != null ? int.Parse(userIdClaim.Value) : (int?)null;
            }
            catch (SecurityTokenExpiredException)
            {
                // Le token est expiré
                return null;
            }
            catch (Exception)
            {
                // Token invalide
                return null;
            }
        }




        [HttpPatch("profile")]
        [Authorize]
        public async Task<IActionResult> PatchProfile([FromBody] JsonPatchDocument<User> patchDoc)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int id))
            {
                return BadRequest("Invalid user ID format");
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            patchDoc.ApplyTo(user, (Microsoft.AspNetCore.JsonPatch.Adapters.IObjectAdapter)ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("Concurrency conflict occurred");
            }

            return Ok(user);
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } 
        public string Password { get; set; }  
    }
}
