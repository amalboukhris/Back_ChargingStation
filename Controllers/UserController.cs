using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ChargingStation.Data;
using ChargingStation.Models;
using Microsoft.AspNetCore.Authorization;
using BCrypt.Net;
using Microsoft.Extensions.Logging;

using VehicleChargingStation.Dto;

namespace ChargingStation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserController> _logger;

        public UserController(
            AppDbContext context,
            ILogger<UserController> logger,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDto registrationDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _context.Users.AnyAsync(u => u.Email == registrationDto.Email))
                return BadRequest(new { Message = "Email is already taken." });

            var user = new User
            {
                FirstName = registrationDto.FirstName,
                LastName = registrationDto.LastName,
                Email = registrationDto.Email,
                PhoneNumber = registrationDto.PhoneNumber,
                DateOfBirth = registrationDto.DateOfBirth,
                Password = BCrypt.Net.BCrypt.HashPassword(registrationDto.Password),
                Role = "Client"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "User registered successfully" });
        }

        [HttpPost("register-admin")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] UserRegistrationDto registrationDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _context.Users.AnyAsync(u => u.Email == registrationDto.Email))
                return BadRequest(new { Message = "Email is already taken." });

            var admin = new Admin
            {
                FirstName = registrationDto.FirstName,
                LastName = registrationDto.LastName,
                Email = registrationDto.Email,
                PhoneNumber = registrationDto.PhoneNumber,
                DateOfBirth = registrationDto.DateOfBirth,
                Password = BCrypt.Net.BCrypt.HashPassword(registrationDto.Password),
                Role = "Admin"
            };

            _context.Users.Add(admin);
            await _context.SaveChangesAsync();

            // Explicitly add to Admin role if using ASP.NET Core Identity
            // await _userManager.AddToRoleAsync(admin, "Admin");

            return Ok(new
            {
                Message = "Admin registered successfully",
                AdminId = admin.Id
            });
        }

        [HttpPost("login-admin")]
        public async Task<ActionResult> LoginAdmin([FromBody] LoginRequest loginRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _logger.LogInformation("Login attempt for {Email}", loginRequest.Email);

                // First check if any user exists with this email
                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == loginRequest.Email);

                if (user == null)
                {
                    _logger.LogWarning("No user found with email {Email}", loginRequest.Email);
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                // Then verify if it's actually an Admin
                var admin = await _context.Users
                    .OfType<Admin>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == user.Id);

                if (admin == null)
                {
                    _logger.LogWarning("User {Email} is not an admin", loginRequest.Email);
                    return Unauthorized(new { message = "Admin access only" });
                }

                if (!BCrypt.Net.BCrypt.Verify(loginRequest.Password, admin.Password))
                {
                    _logger.LogWarning("Invalid password for {Email}", loginRequest.Email);
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                _logger.LogInformation("Generating token for admin {Id}", admin.Id);
                var token = GenerateJwtToken(admin);

                return Ok(new
                {
                    token,
                    user = new
                    {
                        id = admin.Id,
                        email = admin.Email,
                        firstName = admin.FirstName,
                        lastName = admin.LastName,
                        role = admin.Role
                    },
                    expiresIn = 3600
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin login error for {Email}", loginRequest.Email);
                return StatusCode(500, new
                {
                    message = "Login failed",
                    detail = ex.Message // Only include in development!
                });
            }
        }
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _logger.LogInformation("Login attempt with email: {Email}", loginRequest?.Email);

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == loginRequest.Email);

                try
                {
                    if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.Password))
                        return Unauthorized(new { Message = "Invalid email or password." });
                }
                catch (Exception bcryptEx)
                {
                    _logger.LogError(bcryptEx, "Error during password verification");
                    return StatusCode(500, new { Message = "Error verifying password" });
                }


                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.GivenName, user.FirstName),
                    new Claim(ClaimTypes.Surname, user.LastName),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("UserType", user.GetType().Name)
                };

                var token = GenerateJwtToken(user);

                return Ok(new
                {
                    Token = token,
                    UserId = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role,
                    UserType = user.GetType().Name,
                    ExpiresIn = TimeSpan.FromDays(7).TotalSeconds
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in user login");
                return StatusCode(500, new { Message = ex.Message }); // Pour voir le message réel
            }
        }

        private string GenerateJwtToken(User user)
        {
            try
            {
                var jwtKey = _configuration["Jwt:SecretKey"] ?? throw new ArgumentNullException("JWT Key is not configured");
                var jwtIssuer = _configuration["Jwt:Issuer"] ?? throw new ArgumentNullException("JWT Issuer is not configured");
                var jwtAudience = _configuration["Jwt:Audience"] ?? throw new ArgumentNullException("JWT Audience is not configured");
                var expiryMinutes = _configuration.GetValue<int>("Jwt:ExpiryInMinutes", 60);

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

                var token = new JwtSecurityToken(
                    issuer: jwtIssuer,
                    audience: jwtAudience,
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(expiryMinutes),
                    signingCredentials: credentials);

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate JWT token");
                throw; // Or handle differently based on your requirements
            }
        }
        [Authorize]
        [HttpPut("update-fcm-token")]
        public async Task<IActionResult> UpdateFcmToken([FromBody] FcmTokenDto dto)
        {
            var userId = int.Parse(User.FindFirst("id")!.Value);
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.FcmToken = dto.FcmToken;
            await _context.SaveChangesAsync();

            return Ok();
        }

        public class FcmTokenDto
        {
            public string FcmToken { get; set; } = string.Empty;
        }

        [HttpGet("users")]
        [Authorize(Roles = "Admin")] // Restrict to admin users only
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAllUsers()
        {
            var users = await _context.Users
                .AsNoTracking()
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    DateOfBirth = u.DateOfBirth,
                    Role = u.Role
                })
                .ToListAsync();

            return Ok(users);
        }
        [HttpGet("verify-token")]
        [Authorize]
        public IActionResult VerifyToken()
        {
            // If execution reaches here, token is valid (thanks to [Authorize] attribute)
            return Ok(new { message = "Token is valid" });
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
        [HttpGet("users/{userId}/notifications")]
        public async Task<IActionResult> GetUserNotifications(int userId)
        {
            var notifications = await _context.Reservations
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.StartTime)
                .Select(r => new ReservationNotificationDto(r))
                .ToListAsync();

            return Ok(notifications);
        }

    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class UserRegistrationDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Password { get; set; }
    }

    public class UserUpdateDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }

    public class UserResponseDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Role { get; set; }
    }

    public class AdminDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string DateOfBirth { get; set; }
    }
}