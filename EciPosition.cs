using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace SpaceTrack.DAL.Model
{
    [BsonIgnoreExtraElements]
    public class EciPosition
    {
        public string ObjectId { get; set; }  // Satellite/Debris ID
        [BsonElement("NoradId")]
        public int NoradId { get; set; }
        [BsonElement("ObjectType")]
        public string ObjectType { get; set; } // Satellite, Debris, etc.
        [BsonElement("FirstLine")]
        public string FirstLine { get; set; } // First line of TLE

        [BsonElement("SecondLine")]
        public string SecondLine { get; set; } // Second line of TLE
        [BsonElement("UtcTime")]

        public DateTime UtcTime { get; set; } // Time of ECI computation
        [BsonElement("x")]
        public double X { get; set; }  // ECI X-coordinate (km)
        [BsonElement("y")]
        public double Y { get; set; }  // ECI Y-coordinate (km)
        [BsonElement("z")]
        public double Z { get; set; }  // ECI Z-coordinate (km)
        [BsonElement("vx")]
        public double Vx { get; set; } // ECI X-velocity (km/s)
        [BsonElement("Vy")]
        public double Vy { get; set; } // ECI Y-velocity (km/s)
        [BsonElement("Vz")]
        public double Vz { get; set; } // ECI Z-velocity (km/s)
    }
}
