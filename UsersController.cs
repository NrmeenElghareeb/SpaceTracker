using Microsoft.AspNetCore.Mvc;
using SpaceTrack.DAL.Model;
using SpaceTrack.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using SpaceTrack.DAL.DTOs;
using MongoDB.Driver;
using SpaceTrack.DAL;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Session;
using System.Data;
namespace SpaceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }
        
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserDTO dto)
        {
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password); // Hash the password

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Password = hashedPassword // Save the hashed password
            };

            var result = await _userService.Register(user);
            if (result == null)
                return BadRequest("User already exists.");

            return Ok("Registration successful! Welcome to our platform. You can now log in and start exploring.");
        }
        
        [HttpPost("login")]
        public async Task<IActionResult> Login(string name, string password)
        {
           
            var user = await _userService.Login(name, password);
            if (user == null)
                return Unauthorized("Invalid credentials. User not found.");
            
            // Generate JWT token if credentials are valid
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("JSAHGrinvfcroe480943kdsfskfpe??fti0943wjdwoiejfoj23484"); // Use a secure key
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)

        }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new { Token = tokenString });
        }

        [Authorize(Roles = "admin")]
        [HttpGet("all-users")]
        public async Task<IActionResult> GetAllUsers()
        {
            return Ok(await _userService.GetAllUsers());
        }
       
        [Authorize(Roles = "admin")]
        [HttpPost("add-user")]
        public async Task<IActionResult> AddUser(PostuserDTO dto)
        {
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password); // Hash the password

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Password = hashedPassword, // Save the hashed password
                Role = dto.Role,
            };

            var result = await _userService.AddUser(user);
            if (result == null)
                return BadRequest("User already exists.");

            return Ok("User added successfully.");
        }

        [Authorize(Roles = "admin")]
        [HttpPut("update-user")]
        public async Task<IActionResult> UpdateUser(updateuserdto dto)
        {
            // Hash the password only if it's provided
            string hashedPassword = null;
            if (!string.IsNullOrEmpty(dto.Password))
            {
                hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }
            var updatedUser = new User
            {
                Id = dto.Id,
                Name = dto.Name,
                Email = dto.Email,
                Password = hashedPassword, // Use hashed password
                Role = string.IsNullOrEmpty(dto.Role) ? "user" : dto.Role, // Default to "user" if no role is provided
                SatelliteNames = dto.SatelliteNames,
                ResetToken = dto.ResetToken,
                ResetTokenExpires = dto.ResetTokenExpires
            };
            // Call the service to update the user
            var result = await _userService.UpdateUserAsync(updatedUser);
            if (result == null)
            {
                return NotFound("User not found.");
            }

            return Ok("User updated successfully.");
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("delete-user/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            await _userService.DeleteUser(id);
            return Ok("User deleted successfully.");
        }

        [Authorize(Roles = "admin")]
        // GET: api/users/email/{email}
        [HttpGet("email/{email}")]
            public async Task<IActionResult> GetUserByEmail(string email)
            {
                // Validate the input
                if (string.IsNullOrWhiteSpace(email))
                {
                    return BadRequest("Email cannot be null or empty.");
                }

                // Call the service to get the user
                var user = await _userService.GetUserByEmailAsync(email);

                // Check if user was found
                if (user == null)
                {
                    return NotFound($"No user found with email: {email}");
                }

                // Return the user
                return Ok(user);
            }
        

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] string email)
        {
            var result = await _userService.ForgotPasswordAsync(email);
            if (!result) return BadRequest("User not found or email is invalid.");

            return Ok("Reset password link sent to your email.");
        }
       
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest model)
        {
            var result = await _userService.ResetPasswordAsync(model.Token, model.NewPassword);
            if (!result) return BadRequest("Invalid token or token expired.");

            return Ok("Password has been reset successfully.");
        }
        // Endpoint for users to send a message
        [HttpPost("Sendmessage")]
        public async Task<IActionResult> SendMessage([FromBody] ContactMessage message)
        {
            if (message == null || string.IsNullOrEmpty(message.Email) || string.IsNullOrEmpty(message.Message))
            {
                return BadRequest("Invalid message data.");
            }

            await _userService.SendMessage(message);
            return Ok("Message sent successfully.");
        }
        // Endpoint for admin to retrieve all messages
        [Authorize(Roles = "admin")]
        [HttpGet("Getmessages")]
        public async Task<IActionResult> GetAllMessages()
        {
            var messages = await _userService.GetAllMessages();
            return Ok(messages);
        }

    }
}



