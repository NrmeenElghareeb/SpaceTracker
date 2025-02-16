using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceTrack.DAL.Model
{
    public class User
    {
        [BsonId]
        [BsonElement("_id"),BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string? Id { get; set; }
        // Name with validation: Only letters and spaces, minimum 2 characters, max 50.
        [Required(ErrorMessage = "Name is required.")]
        [RegularExpression(@"^[a-zA-Z\s]{2,50}$", ErrorMessage = "Name can only contain letters and spaces, with 2 to 50 characters.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
           ErrorMessage = "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
        public string Password { get; set; }
        public List<string> SatelliteNames { get; set; } = new();
        public string Role { get; set; }
        // Last login time (automatically set to current time)
        private DateTime _lastLoginTime = DateTime.UtcNow;
        public DateTime LastLoginTime
        {
            get => _lastLoginTime;
            set => _lastLoginTime = value == default ? DateTime.UtcNow : value;
        }
        [Required(ErrorMessage = "Email is required.")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }
        public string ResetToken { get; set; }
        public DateTime? ResetTokenExpires { get; set; }

    }
}
