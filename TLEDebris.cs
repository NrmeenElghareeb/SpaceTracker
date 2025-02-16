using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace SpaceTrack.DAL.Model
{
    public class TLEDebris
    {
        


        ///////////////////////////////////////////////////////////////////////////////////////Excellent
        [BsonId]
        [BsonElement("_id"), BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("NoradId")]
        public int NoradId { get; set; }

        [BsonElement("FirstLine")]
        public string FirstLine { get; set; } // First line of TLE

        [BsonElement("SecondLine")]
        public string SecondLine { get; set; } // Second line of TLE

        [BsonElement("Timestamp")]
        public DateTime Timestamp { get; set; }
        [BsonElement("Eci")]
        public EciPosition Eci { get; set; } = new();
        ///////////////////////////////////////////////////////////////////////////////////////Excellent


        /////////////////////////////////////////change 1
        //[BsonId]
        //[BsonElement("_id"), BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        //public string? Id { get; set; }
        //[BsonElement("NoradId")]
        //public int NoradId { get; set; }

        //public string FirstLine { get; set; } // First line of TLE
        //public string SecondLine { get; set; }

        //[BsonElement("Timestamp")]
        //public DateTime Timestamp { get; set; }
        /////////////////////////////////////////////////////////////change 1

        //[BsonId]
        //[BsonElement("_id"), BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        //public string? Id { get; set; }

        //[BsonElement("NoradId")]
        //public int NoradId { get; set; }

        //[BsonElement("Line1")]
        //public string Line1 { get; set; }

        //[BsonElement("Line2")]
        //public string Line2 { get; set; }

        //[BsonElement("RetrievedAt")]
        //public DateTime RetrievedAt { get; set; }

    }
}



