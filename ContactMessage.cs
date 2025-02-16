using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SpaceTrack.DAL.Model
{
    public class ContactMessage
    {
       
        [BsonId]
        [BsonElement("_id"), BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string? Id { get; set; }
        [Required(ErrorMessage = "Name is required.")]
        [RegularExpression(@"^[a-zA-Z\s]{2,50}$", ErrorMessage = "Name can only contain letters and spaces, with 2 to 50 characters.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Email is required.")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }
        public string Message { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
