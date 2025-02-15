using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using SpaceTrack.DAL;
using SpaceTrack.DAL.Model;
using System.Linq;
using System.Net.Mail;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Net;
using static System.Net.WebRequestMethods;

namespace SpaceTrack.Services
{
    public class UserService : IUserService
    {
        private readonly MongoDbContext _context;

        public UserService(MongoDbContext context)
        {
            _context = context;
        }

        // Register a new user
           public async Task<User> Register(User userInput)
        {
            // Check if user with same email exists
            var existingUser = await _context.Users.Find(u => u.Email == userInput.Email).FirstOrDefaultAsync();
            if (existingUser != null)
                return null; // User already exists
            // Create a new User object with only the required fields
            var newUser = new User
            {
                Name = userInput.Name,
                Email = userInput.Email,
                Password = userInput.Password, // You might want to hash the password here
                Role = "user", // Default role assignment
                SatelliteNames = new List<string>(), // Initialize empty list
                LastLoginTime = DateTime.UtcNow // Set default login time
            };

            // Insert the new user into the database
            await _context.Users.InsertOneAsync(newUser);

            return newUser;
        }
        public async Task<User>AddUser(User userInput)
        {
            var existingUser = await _context.Users.Find(u => u.Email == userInput.Email).FirstOrDefaultAsync();
            if (existingUser != null)
                return null; // User already exists
            // Create a new User object with only the required fields
            var newUser = new User
            {
                Name = userInput.Name,
                Email = userInput.Email,
                Password = userInput.Password, // You might want to hash the password here
                Role = (userInput.Role == "admin" || userInput.Role == "user")
       ? userInput.Role
       : "user", // Default to "user" if invalid role provided
                 // Default role assignment
                SatelliteNames = new List<string>(), // Initialize empty list
                LastLoginTime = DateTime.UtcNow // Set default login time
            };

            // Insert the new user into the database
            await _context.Users.InsertOneAsync(newUser);

            return newUser;
        }
        public async Task<User> Login(string name, string password)
        {

            var filter = Builders<User>.Filter.Eq(u => u.Name, name);
            var user = await _context.Users.Find(filter).FirstOrDefaultAsync();

            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return user;
            }
            return null;

        }

        // Get all users
        public async Task<List<User>> GetAllUsers()
        {
            return await _context.Users.Find(_ => true).ToListAsync();
        }

        public async Task<User> UpdateUserAsync(User updatedUser)
        {
            // Find the existing user by ID
            var filter = Builders<User>.Filter.Eq(u => u.Id, updatedUser.Id);

            // Update the user fields (excluding null or empty values)
            var updateDefinition = Builders<User>.Update
                .Set(u => u.Name, updatedUser.Name)
                .Set(u => u.Email, updatedUser.Email);

            if (!string.IsNullOrEmpty(updatedUser.Password))
            {
                updateDefinition = updateDefinition.Set(u => u.Password, updatedUser.Password);
            }
            updateDefinition = updateDefinition
                .Set(u => u.Role, updatedUser.Role)
                .Set(u => u.SatelliteNames, updatedUser.SatelliteNames)
                .Set(u => u.ResetToken, updatedUser.ResetToken)
                .Set(u => u.ResetTokenExpires, updatedUser.ResetTokenExpires);
            // Apply the update
            var result = await _context.Users.UpdateOneAsync(filter, new UpdateDefinitionBuilder<User>().Combine(updateDefinition));
            if (result.MatchedCount == 0)
            {
                // No user found with the given ID
                return null;
            }
            // Fetch and return the updated user
            return await _context.Users.Find(filter).FirstOrDefaultAsync();
        }

        public async Task DeleteUser(string userId)
        {
            await _context.Users.DeleteOneAsync(u => u.Id == userId);
        }
        public async Task<User> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email cannot be null or empty", nameof(email));
            }

            return await _context.Users
                .Find(u => u.Email == email)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var user = _context.Users.AsQueryable().FirstOrDefault(u => u.Email == email);
            if (user == null) return false;
            // Generate a 6-digit numeric code
            var random = new Random();
            var resetCode = random.Next(100000, 999999).ToString();
            // Save the code and expiry in the user document
            user.ResetToken = resetCode;
            user.ResetTokenExpires = DateTime.UtcNow.AddHours(1);
            await _context.Users.ReplaceOneAsync(u => u.Email == user.Email, user);
            //Send the reset code using SMTP
            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")  // Change SMTP server if not using Gmail
                {
                    Port = 587, // Port for Gmail SMTP
                    Credentials = new NetworkCredential("mohamedsalah197300@gmail.com", "siav pxlj eigu batl"),
                    EnableSsl = true // Enables SSL encryption for secure email sending
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("mohamedsalah197300@gmail.com", "SpaceTrack"),
                    Subject = "Password Reset Code",
                    //Body = $"Your password reset code is: {resetCode}"+ $"<a href='https://yourdomain.com/reset'>Reset Password</a>",
                    Body = $"Your password reset code is: {resetCode}<br>" +
                           $"<a href='https://yourdomain.com/reset'>Reset Password</a>",


                    IsBodyHtml = true, // Set true if you want to send HTML content
                };

                mailMessage.To.Add(user.Email);
                await smtpClient.SendMailAsync(mailMessage);
                return true;
            }

            ////////////////////
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                return false;
            } 

        }
 /*
Authentication:The API key allows your application to connect to SendGrid's services. Without this key, SendGrid will reject your email-sending requests.
Security:The API key is a secret token tied to your SendGrid account. It ensures that only authorized applications or users can send emails via your SendGrid account. */

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var user = _context.Users.AsQueryable().FirstOrDefault(u => u.ResetToken == token);
            if (user == null || user.ResetTokenExpires < DateTime.UtcNow) return false;

            // Reset password
            user.ResetToken = null; // Clear the reset token
            user.ResetTokenExpires = null;
            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword); // Hash the new password

            await _context.Users.ReplaceOneAsync(u => u.Email == user.Email, user);

            return true;
        }
        // Allow users to send a message
        public async Task SendMessage(ContactMessage message)
        {
            message.CreatedAt = DateTime.UtcNow; // Ensure timestamp is set
            await _context.ContactMessages.InsertOneAsync(message); // Save to the MongoDB collection
        }

        // Allow admin to retrieve all messages
        public async Task<List<ContactMessage>> GetAllMessages()
        {
            var messages = await _context.ContactMessages.Find(_ => true).ToListAsync(); // Fetch all contact messages
            return messages;
        }

    }
}

