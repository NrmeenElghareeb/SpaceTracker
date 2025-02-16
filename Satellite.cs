using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SpaceTrack.DAL.Model
{
    public class Satellite
    {
       
        [BsonId]
        [BsonElement("_id"), BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string? Id { get; set; }  // MongoDB generated unique ID

        [BsonElement("NoardId")]
        public string NoardId { get; set; }

        [BsonElement("Name")]
        public string Name { get; set; }
        [BsonElement("Eci")]
        public EciPosition Eci { get; set; } = new();
    }
}
