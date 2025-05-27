using System.Security.Cryptography;
using System.Text;
using MarketMinds.Shared.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UsersController> _logger;
        private const int BuyerTypeValue = 1;
        private const int BaseBalance = 0;
        private const int BaseRating = 0;

        public UsersController(IUserRepository userRepository, ILogger<UsersController> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(object), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Username) ||
                string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("User data is incomplete.");
            }

            try
            {
                if (await _userRepository.UsernameExistsAsync(request.Username))
                {
                    return Conflict("Username is already taken.");
                }

                if (await _userRepository.EmailExistsAsync(request.Email))
                {
                    return Conflict("Email is already registered.");
                }

                var user = await _userRepository.RegisterUserAsync(
                    request.Username,
                    request.Email,
                    HashPassword(request.Password),
                    request.PhoneNumber,
                    request.UserType,
                    null,
                    false,
                    0
                );

                var userResponse = new
                {
                    user.Id,
                    user.Username,
                    user.Email,
                    user.UserType,
                    user.Balance,
                    user.Rating
                };

                return CreatedAtAction(nameof(CheckUsername), new { username = user.Username }, userResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user.");
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Login credentials are required.");
            }

            try
            {
                var user = await _userRepository.FindUserByUsernameAsync(request.Username);

                if (user == null)
                {
                    return NotFound("User not found.");
                }

                if (!VerifyPassword(request.Password, user.PasswordHash))
                {
                    return Unauthorized("Invalid credentials.");
                }

                var userResponse = new
                {
                    user.Id,
                    user.Username,
                    user.Email,
                    user.UserType,
                    user.Balance,
                    user.Rating
                };

                return Ok(userResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login.");
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        [HttpGet("check-username/{username}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CheckUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return BadRequest("Username is required.");
            }

            try
            {
                bool exists = await _userRepository.UsernameExistsAsync(username);
                return Ok(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking username.");
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(hashedBytes).Replace("-", string.Empty).ToLower();
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            string hashedInput = HashPassword(password);
            return hashedInput == hashedPassword;
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class RegisterRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public int UserType { get; set; }
    }
}
