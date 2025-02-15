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
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Options;
using System.Runtime;
namespace SpaceTrack.Services
{
    public class TLEService : ITLEService
    {
        private readonly MongoDbContext _context;
        private readonly SpaceTrackTLE _spaceTrack;
        private readonly IEciService _eciService;

        public TLEService(MongoDbContext context, IEciService eciService)
        {
            _context = context;
            _spaceTrack = new SpaceTrackTLE();
            _eciService = eciService;
        }
        public async Task SaveOrUpdateAllPayloadsTLEsAsync()

        {
            string payloadQueryUrl = "https://www.space-track.org/basicspacedata/query/class/tle_latest/OBJECT_TYPE/PAYLOAD/orderby/ORDINAL%20asc/limit/1000/format/tle/emptyresult/show";

            // Fetch all payload TLE data
            var (firstLines, secondLines) = await _spaceTrack.GetAllTLEDataAsync(payloadQueryUrl);

            for (int i = 0; i < firstLines.Count; i++)
            {
                if (!string.IsNullOrEmpty(firstLines[i]) && !string.IsNullOrEmpty(secondLines[i]))
                {
                    // Parse NoradId from Line1 (3rd to 7th characters are NORAD ID in TLE format)
                    int noradId = int.Parse(firstLines[i].Substring(2, 5).Trim());

                    // Fetch OBJECT_NAME using NORAD ID
                    //string objectName = await _spaceTrack.GetPayloadNameAsync(noradId);

                    var tle = new TLEPayloads
                    {
                        NoradId = noradId,
                        FirstLine = firstLines[i],
                        SecondLine = secondLines[i],
                        // ObjectName = objectName, // Add the fetched name here
                        Timestamp = DateTime.UtcNow
                    };

                    // Check if the TLE data already exists in the database
                    var existingTLE = _context.TLEPAYLOADS.Find(t => t.NoradId == tle.NoradId).FirstOrDefault();

                    if (existingTLE != null)
                    {
                        // Update the existing TLE document
                        var updateDefinition = Builders<TLEPayloads>.Update
                            .Set(t => t.FirstLine, tle.FirstLine)
                            .Set(t => t.SecondLine, tle.SecondLine)
                            .Set(t => t.Timestamp, tle.Timestamp);
                        //.Set(t => t.ObjectName, tle.ObjectName)

                        await _context.TLEPAYLOADS.UpdateOneAsync(t => t.NoradId == tle.NoradId, updateDefinition);
                    }
                    else
                    {
                        // Insert a new TLE document
                        await _context.TLEPAYLOADS.InsertOneAsync(tle);
                    }
                    // compute ECI
                    await _eciService.ComputeEciAsync(tle.ObjectName, tle.NoradId, tle.FirstLine, tle.SecondLine, tle.Timestamp);


                }
            }
        }

        public async Task SaveOrUpdateAllDebrisTLEsAsync()
        {
            string debrisQueryUrl = "https://www.space-track.org/basicspacedata/query/class/tle_latest/OBJECT_TYPE/DEBRIS/orderby/ORDINAL%20asc/limit/1000/format/tle/emptyresult/show";
            var (firstLines, secondLines) = await _spaceTrack.GetAllTLEDataAsync(debrisQueryUrl);


            for (int i = 0; i < firstLines.Count; i++)
            {
                if (!string.IsNullOrEmpty(firstLines[i]) && !string.IsNullOrEmpty(secondLines[i]))
                {
                    // Parse NoradId from Line1 (11th to 16th characters are NORAD ID in TLE format)
                    int noradId = int.Parse(firstLines[i].Substring(2, 5).Trim());

                    var tle = new TLEDebris
                    {
                        NoradId = noradId,
                        FirstLine = firstLines[i],
                        SecondLine = secondLines[i],
                        Timestamp = DateTime.UtcNow
                    };

                    // Check if the TLE data already exists in the database
                    var existingTLE = _context.TLEs.Find(t => t.NoradId == tle.NoradId).FirstOrDefault();

                    if (existingTLE != null)
                    {
                        // Update the existing TLE document
                        var updateDefinition = Builders<TLEDebris>.Update
                            .Set(t => t.FirstLine, tle.FirstLine)
                            .Set(t => t.SecondLine, tle.SecondLine)
                            .Set(t => t.Timestamp, tle.Timestamp);

                        await _context.TLEs.UpdateOneAsync(t => t.NoradId == tle.NoradId, updateDefinition);
                    }
                    else
                    {
                        // Insert a new TLE document
                        await _context.TLEs.InsertOneAsync(tle);
                    }
                    // compute ECI
                    await _eciService.ComputeEciAsync(null, tle.NoradId, tle.FirstLine, tle.SecondLine, tle.Timestamp);

                }
            }

        }
        public async Task SaveOrUpdateAllRocketsTLEsAsync()
        {
            string RocketsQueryUrl = "https://www.space-track.org/basicspacedata/query/class/tle_latest/OBJECT_TYPE/ROCKET%20BODY/orderby/ORDINAL%20asc/limit/10000/format/tle/emptyresult/show";
            // Fetch all debris TLE data
            var (firstLines, secondLines) = await _spaceTrack.GetAllTLEDataAsync(RocketsQueryUrl);

            for (int i = 0; i < firstLines.Count; i++)
            {
                if (!string.IsNullOrEmpty(firstLines[i]) && !string.IsNullOrEmpty(secondLines[i]))
                {
                    // Parse NoradId from Line1 (11th to 16th characters are NORAD ID in TLE format)
                    int noradId = int.Parse(firstLines[i].Substring(2, 5).Trim());

                    var tle = new TLERockets
                    {
                        NoradId = noradId,
                        FirstLine = firstLines[i],
                        SecondLine = secondLines[i],
                        Timestamp = DateTime.UtcNow
                    };

                    // Check if the TLE data already exists in the database
                    var existingTLE = _context.TLEROCKETS.Find(t => t.NoradId == tle.NoradId).FirstOrDefault();

                    if (existingTLE != null)
                    {
                        // Update the existing TLE document
                        var updateDefinition = Builders<TLERockets>.Update
                            .Set(t => t.FirstLine, tle.FirstLine)
                            .Set(t => t.SecondLine, tle.SecondLine)
                            .Set(t => t.Timestamp, tle.Timestamp);

                        await _context.TLEROCKETS.UpdateOneAsync(t => t.NoradId == tle.NoradId, updateDefinition);
                    }
                    else
                    {
                        // Insert a new TLE document
                        await _context.TLEROCKETS.InsertOneAsync(tle);
                    }
                    await _eciService.ComputeEciAsync(null, tle.NoradId, tle.FirstLine, tle.SecondLine, tle.Timestamp);

                }
            }
        }
        public async Task SaveOrUpdateAllUnknownTLEsAsync()
        {
            string UnknownObjectQueryUrl = "https://www.space-track.org/basicspacedata/query/class/tle_latest/OBJECT_TYPE/UNKNOWN/orderby/ORDINAL%20asc/limit/10000/format/tle/emptyresult/show";

            // Fetch all debris TLE data//Unknown //
            var (firstLines, secondLines) = await _spaceTrack.GetAllTLEDataAsync(UnknownObjectQueryUrl);

            for (int i = 0; i < firstLines.Count; i++)
            {
                if (!string.IsNullOrEmpty(firstLines[i]) && !string.IsNullOrEmpty(secondLines[i]))
                {
                    // Parse NoradId from Line1 (11th to 16th characters are NORAD ID in TLE format)
                    int noradId = int.Parse(firstLines[i].Substring(2, 5).Trim());

                    var tle = new TLEUnknown
                    {
                        NoradId = noradId,
                        FirstLine = firstLines[i],
                        SecondLine = secondLines[i],
                        Timestamp = DateTime.UtcNow
                    };

                    // Check if the TLE data already exists in the database
                    var existingTLE = _context.TLEUNKNOWN.Find(t => t.NoradId == tle.NoradId).FirstOrDefault();

                    if (existingTLE != null)
                    {
                        // Update the existing TLE document
                        var updateDefinition = Builders<TLEUnknown>.Update
                            .Set(t => t.FirstLine, tle.FirstLine)
                            .Set(t => t.SecondLine, tle.SecondLine)
                            .Set(t => t.Timestamp, tle.Timestamp);

                        await _context.TLEUNKNOWN.UpdateOneAsync(t => t.NoradId == tle.NoradId, updateDefinition);
                    }
                    else
                    {
                        // Insert a new TLE document
                        await _context.TLEUNKNOWN.InsertOneAsync(tle);
                    }
                    await _eciService.ComputeEciAsync(null , tle.NoradId, tle.FirstLine, tle.SecondLine, tle.Timestamp);

                }
            }

            /*
            public async Task SaveOrUpdateAllPayloadsTLEsAsync()
            {
                string payloadQueryUrl = "https://www.space-track.org/basicspacedata/query/class/tle_latest/OBJECT_TYPE/PAYLOAD/orderby/ORDINAL%20asc/limit/1000/format/tle/emptyresult/show";

                // Fetch all payload TLE data
                var (firstLines, secondLines) = await _spaceTrack.GetAllTLEDataAsync(payloadQueryUrl);

                for (int i = 0; i < firstLines.Count; i++)
                {
                    if (!string.IsNullOrEmpty(firstLines[i]) && !string.IsNullOrEmpty(secondLines[i]))
                    {
                        // Parse NoradId from Line1 (11th to 16th characters are NORAD ID in TLE format)
                        int noradId = int.Parse(firstLines[i].Substring(2, 5).Trim());

                        // Fetch the payload name based on NoradId
                        string payloadName = await _spaceTrack.GetPayloadNameAsync(noradId);

                        var tle = new TLEPayloads
                        {
                            NoradId = noradId,
                            Name = payloadName, // Save the payload name
                            FirstLine = firstLines[i],
                            SecondLine = secondLines[i],
                            Timestamp = DateTime.UtcNow
                        };

                        // Check if the TLE data already exists in the database
                        var existingTLE = _context.TLEPAYLOADS.Find(t => t.NoradId == tle.NoradId).FirstOrDefault();

                        if (existingTLE != null)
                        {
                            // Update the existing TLE document
                            var updateDefinition = Builders<TLEPayloads>.Update
                                .Set(t => t.Name, tle.Name)
                                .Set(t => t.FirstLine, tle.FirstLine)
                                .Set(t => t.SecondLine, tle.SecondLine)
                                .Set(t => t.Timestamp, tle.Timestamp);

                            await _context.TLEPAYLOADS.UpdateOneAsync(t => t.NoradId == tle.NoradId, updateDefinition);
                        }
                        else
                        {
                            // Insert a new TLE document
                            await _context.TLEPAYLOADS.InsertOneAsync(tle);
                        }
                    }
                }
            }
            */
        }

        public Task ComputeEciAsync()
        {
            throw new NotImplementedException();
        }
    }
    

}

    
    
             
      

  



/*///////////////////////////////////////////////////comment to update name
namespace SpaceTrack.Services
{
    public class TLEService : ITLEService
    {
        private readonly MongoDbContext _context;
        private readonly SpaceTrackTLE _spaceTrack;

        public TLEService(MongoDbContext context)
        {
            _context = context;
            _spaceTrack = new SpaceTrackTLE();
        }

        public async Task SaveOrUpdateAllPayloadsTLEsAsync()
        {
            string payloadQueryUrl = "https://www.space-track.org/basicspacedata/query/class/tle_latest/OBJECT_TYPE/PAYLOAD/orderby/ORDINAL%20asc/limit/1000/format/tle/emptyresult/show";

            // Fetch all payload TLE data
            var (firstLines, secondLines) = await _spaceTrack.GetAllTLEDataAsync(payloadQueryUrl);

            for (int i = 0; i < firstLines.Count; i++)
            {
                if (!string.IsNullOrEmpty(firstLines[i]) && !string.IsNullOrEmpty(secondLines[i]))
                {
                    // Parse NoradId from Line1 (11th to 16th characters are NORAD ID in TLE format)
                    int noradId = int.Parse(firstLines[i].Substring(2, 5).Trim());

                    var tle = new TLEPayloads
                    {
                        NoradId = noradId,
                        FirstLine = firstLines[i],
                        SecondLine = secondLines[i],
                        Timestamp = DateTime.UtcNow
                    };

                    // Check if the TLE data already exists in the database
                    var existingTLE = _context.TLEPAYLOADS.Find(t => t.NoradId == tle.NoradId).FirstOrDefault();

                    if (existingTLE != null)
                    {
                        // Update the existing TLE document
                        var updateDefinition = Builders<TLEPayloads>.Update
                            .Set(t => t.FirstLine, tle.FirstLine)
                            .Set(t => t.SecondLine, tle.SecondLine)
                            .Set(t => t.Timestamp, tle.Timestamp);

                        await _context.TLEPAYLOADS.UpdateOneAsync(t => t.NoradId == tle.NoradId, updateDefinition);
                    }
                    else
                    {
                        // Insert a new TLE document
                        await _context.TLEPAYLOADS.InsertOneAsync(tle);
                    }
                }
            }
        }
    }
}
*/
/*
namespace SpaceTrack.Services
{
   

    public class TLEService : ITLEService
    {
        private readonly MongoDbContext _context;
        private readonly SpaceTrackTLE _spaceTrack;

        public TLEService(MongoDbContext context)
        {
            _context = context;
            _spaceTrack = new SpaceTrackTLE();
        }

        public async Task SaveOrUpdateAllPayloadsTLEsAsync()
        {
            string payloadQueryUrl = "https://www.space-track.org/basicspacedata/query/class/tle_latest/OBJECT_TYPE/PAYLOAD/orderby/ORDINAL%20asc/limit/11100/format/tle/emptyresult/show";
            // Fetch all debris TLE data
            var (firstLines, secondLines) = _spaceTrack.GetAllTLEData(payloadQueryUrl);

            for (int i = 0; i < firstLines.Count; i++)
            {
                if (!string.IsNullOrEmpty(firstLines[i]) && !string.IsNullOrEmpty(secondLines[i]))
                {
                    // Parse NoradId from Line1 (11th to 16th characters are NORAD ID in TLE format)
                    int noradId = int.Parse(firstLines[i].Substring(2, 5).Trim());

                    var tle = new TLEPayloads
                    {
                        NoradId = noradId,
                        FirstLine = firstLines[i],
                        SecondLine = secondLines[i],
                        Timestamp = DateTime.UtcNow
                    };

                    // Check if the TLE data already exists in the database
                    var existingTLE = _context.TLEPAYLOADS.Find(t => t.NoradId == tle.NoradId).FirstOrDefault();

                    if (existingTLE != null)
                    {
                        // Update the existing TLE document
                        var updateDefinition = Builders<TLEPayloads>.Update
                            .Set(t => t.FirstLine, tle.FirstLine)
                            .Set(t => t.SecondLine, tle.SecondLine)
                            .Set(t => t.Timestamp, tle.Timestamp);

                        await _context.TLEPAYLOADS.UpdateOneAsync(t => t.NoradId == tle.NoradId, updateDefinition);
                    }
                    else
                    {
                        // Insert a new TLE document
                        await _context.TLEPAYLOADS.InsertOneAsync(tle);
                    }
                }
            }
        }


        public async Task SaveOrUpdateAllDebrisTLEsAsync()
        {
            string debrisQueryUrl = "https://www.space-track.org/basicspacedata/query/class/tle_latest/OBJECT_TYPE/DEBRIS/orderby/ORDINAL%20asc/limit/40000/format/tle/emptyresult/show";
            var (firstLines, secondLines) = _spaceTrack.GetAllTLEData(debrisQueryUrl);
           
           
            for (int i = 0; i < firstLines.Count; i++)
            {
                if (!string.IsNullOrEmpty(firstLines[i]) && !string.IsNullOrEmpty(secondLines[i]))
                {
                    // Parse NoradId from Line1 (11th to 16th characters are NORAD ID in TLE format)
                    int noradId = int.Parse(firstLines[i].Substring(2, 5).Trim());

                    var tle = new TLEDebris
                    {
                        NoradId = noradId,
                        FirstLine = firstLines[i],
                        SecondLine = secondLines[i],
                        Timestamp = DateTime.UtcNow
                    };

                    // Check if the TLE data already exists in the database
                    var existingTLE = _context.TLEs.Find(t => t.NoradId == tle.NoradId).FirstOrDefault();

                    if (existingTLE != null)
                    {
                        // Update the existing TLE document
                        var updateDefinition = Builders<TLEDebris>.Update
                            .Set(t => t.FirstLine, tle.FirstLine)
                            .Set(t => t.SecondLine, tle.SecondLine)
                            .Set(t => t.Timestamp, tle.Timestamp);

                        await _context.TLEs.UpdateOneAsync(t => t.NoradId == tle.NoradId, updateDefinition);
                    }
                    else
                    {
                        // Insert a new TLE document
                        await _context.TLEs.InsertOneAsync(tle);
                    }
                }
            }
            
        }

        /////////////////////////////////////////////////////////////////Excellent
        


        public async Task SaveOrUpdateAllRocketsTLEsAsync()
        {
            string RocketsQueryUrl = "https://www.space-track.org/basicspacedata/query/class/tle_latest/OBJECT_TYPE/ROCKET%20BODY/orderby/ORDINAL%20asc/limit/10000/format/tle/emptyresult/show";
            // Fetch all debris TLE data
            var (firstLines, secondLines) = _spaceTrack.GetAllTLEData(RocketsQueryUrl);

            for (int i = 0; i < firstLines.Count; i++)
            {
                if (!string.IsNullOrEmpty(firstLines[i]) && !string.IsNullOrEmpty(secondLines[i]))
                {
                    // Parse NoradId from Line1 (11th to 16th characters are NORAD ID in TLE format)
                    int noradId = int.Parse(firstLines[i].Substring(2, 5).Trim());

                    var tle = new TLERockets
                    {
                        NoradId = noradId,
                        FirstLine = firstLines[i],
                        SecondLine = secondLines[i],
                        Timestamp = DateTime.UtcNow
                    };

                    // Check if the TLE data already exists in the database
                    var existingTLE = _context.TLEROCKETS.Find(t => t.NoradId == tle.NoradId).FirstOrDefault();

                    if (existingTLE != null)
                    {
                        // Update the existing TLE document
                        var updateDefinition = Builders<TLERockets>.Update
                            .Set(t => t.FirstLine, tle.FirstLine)
                            .Set(t => t.SecondLine, tle.SecondLine)
                            .Set(t => t.Timestamp, tle.Timestamp);

                        await _context.TLEROCKETS.UpdateOneAsync(t => t.NoradId == tle.NoradId, updateDefinition);
                    }
                    else
                    {
                        // Insert a new TLE document
                        await _context.TLEROCKETS.InsertOneAsync(tle);
                    }
                }
            }
        }

        public async Task SaveOrUpdateAllUnknownTLEsAsync()
        {
            string UnknownObjectQueryUrl = "https://www.space-track.org/basicspacedata/query/class/tle_latest/OBJECT_TYPE/UNKNOWN/orderby/ORDINAL%20asc/limit/10000/format/tle/emptyresult/show";

            // Fetch all debris TLE data//Unknown //
            var (firstLines, secondLines) = _spaceTrack.GetAllTLEData(UnknownObjectQueryUrl);

            for (int i = 0; i < firstLines.Count; i++)
            {
                if (!string.IsNullOrEmpty(firstLines[i]) && !string.IsNullOrEmpty(secondLines[i]))
                {
                    // Parse NoradId from Line1 (11th to 16th characters are NORAD ID in TLE format)
                    int noradId = int.Parse(firstLines[i].Substring(2, 5).Trim());

                    var tle = new TLEUnknown
                    {
                        NoradId = noradId,
                        FirstLine = firstLines[i],
                        SecondLine = secondLines[i],
                        Timestamp = DateTime.UtcNow
                    };

                    // Check if the TLE data already exists in the database
                    var existingTLE = _context.TLEUNKNOWN.Find(t => t.NoradId == tle.NoradId).FirstOrDefault();

                    if (existingTLE != null)
                    {
                        // Update the existing TLE document
                        var updateDefinition = Builders<TLEUnknown>.Update
                            .Set(t => t.FirstLine, tle.FirstLine)
                            .Set(t => t.SecondLine, tle.SecondLine)
                            .Set(t => t.Timestamp, tle.Timestamp);

                        await _context.TLEUNKNOWN.UpdateOneAsync(t => t.NoradId == tle.NoradId, updateDefinition);
                    }
                    else
                    {
                        // Insert a new TLE document
                        await _context.TLEUNKNOWN.InsertOneAsync(tle);
                    }
                }
            }
        }
    }

}


*/




///////////////////////////////////////////////////////////////////////////////////////////////////////Work only for 24000Debri
/*
 * 
  public async Task SaveOrUpdateAllDebrisTLEsAsync()
   {

       // Fetch all debris TLE data
       var (firstLines, secondLines) = _spaceTrack.GetAllDebrisTLEData();

       for (int i = 0; i < firstLines.Count; i++)
       {
           if (!string.IsNullOrEmpty(firstLines[i]) && !string.IsNullOrEmpty(secondLines[i]))
           {
               // Parse NoradId from Line1 (11th to 16th characters are NORAD ID in TLE format)
               int noradId = int.Parse(firstLines[i].Substring(2, 5).Trim());

               var tle = new TLEDebris
               {
                   NoradId = noradId,
                   FirstLine = firstLines[i],
                   SecondLine = secondLines[i],
                   Timestamp = DateTime.UtcNow
               };

               // Check if the TLE data already exists in the database
               var existingTLE = _context.TLEs.Find(t => t.NoradId == tle.NoradId).FirstOrDefault();

               if (existingTLE != null)
               {
                   // Update the existing TLE document
                   var updateDefinition = Builders<TLEDebris>.Update
                       .Set(t => t.FirstLine, tle.FirstLine)
                       .Set(t => t.SecondLine, tle.SecondLine)
                       .Set(t => t.Timestamp, tle.Timestamp);

                   await _context.TLEs.UpdateOneAsync(t => t.NoradId == tle.NoradId, updateDefinition);
               }
               else
               {
                   // Insert a new TLE document
                   await _context.TLEs.InsertOneAsync(tle);
               }
           }
       }

   }

*/
///////////////////////////////////////////////////////////////////////////////////////////////////////Work only for 24000Debri

/////////////////////////////////////////////////////////////////////////////////////////Excellent

//        public async Task SaveOrUpdateTLEsAsync()
//        {
//            var noradIds = new List<string> { "49260", "25994", "40697", "25544" }; // List of NORAD IDs
//            DateTime startDate = DateTime.UtcNow.AddDays(-7); // Fetch recent TLE
//            DateTime endDate = DateTime.UtcNow;

//            foreach (var noradId in noradIds)
//            {
//                var (firstLine, secondLine) = _spaceTrack.GetTLEData(noradId, startDate, endDate);
//                if (!string.IsNullOrEmpty(firstLine) && !string.IsNullOrEmpty(secondLine))
//                {
//                    var tle = new TLE
//                    {
//                        NoradId = int.Parse(noradId),
//                        FirstLine = firstLine,
//                        SecondLine = secondLine,
//                        Timestamp = DateTime.UtcNow
//                    };

//                    // Check if the TLE data already exists in the database
//                    var existingTLE = _context.TLEs.Find(t => t.NoradId == tle.NoradId).FirstOrDefault();

//                    if (existingTLE != null)
//                    {
//                        // Update the existing TLE document
//                        var updateDefinition = Builders<TLE>.Update
//                            .Set(t => t.FirstLine, tle.FirstLine)
//                            .Set(t => t.SecondLine, tle.SecondLine)
//                            .Set(t => t.Timestamp, tle.Timestamp);

//                        await _context.TLEs.UpdateOneAsync(t => t.NoradId == tle.NoradId, updateDefinition);
//                    }
//                    else
//                    {
//                        // Insert a new TLE document
//                        await _context.TLEs.InsertOneAsync(tle);
//                    }
//                }
//            }
//        }
//    }
//}
/////////////////////////////////////////////////////////////////////////////////////////Excellent


///////////////////////////////////////////Change 3
//public async Task SaveOrUpdateTLEOfISSAsync()
//{
//    string noradId = "25544"; // NORAD ID of ISS
//    DateTime startDate = DateTime.UtcNow.AddDays(-7); // Fetch recent TLE
//    DateTime endDate = DateTime.UtcNow;

//    var (firstLine, secondLine) = _spaceTrack.GetTLEData(noradId, startDate, endDate);
//    if (!string.IsNullOrEmpty(firstLine) && !string.IsNullOrEmpty(secondLine))
//    {
//        var tle = new TLE
//        {
//            NoradId = int.Parse(noradId),
//            FirstLine = firstLine,
//            SecondLine = secondLine,
//            Timestamp = DateTime.UtcNow
//        };

//        // Check if the TLE data already exists in the database
//        var existingTLE = _context.TLEs.Find(t => t.NoradId == tle.NoradId).FirstOrDefault();

//        if (existingTLE != null)
//        {
//            // Update the existing TLE document
//            var updateDefinition = Builders<TLE>.Update
//                .Set(t => t.FirstLine, tle.FirstLine)
//                .Set(t => t.SecondLine, tle.SecondLine)
//                .Set(t => t.Timestamp, tle.Timestamp);

//            await _context.TLEs.UpdateOneAsync(t => t.NoradId == tle.NoradId, updateDefinition);
//        }
//        else
//        {
//            // Insert a new TLE document
//            await _context.TLEs.InsertOneAsync(tle);
//        }
//    }
//}
//////////////////////////////////////////////////////Change 3

//public async Task SaveTLEOfISSAsync()////////////////////////////////////////cahnge 2
//{
//    string noradId = "25544"; // NORAD ID of ISS
//    DateTime startDate = DateTime.UtcNow.AddDays(-7); // Fetch recent TLE
//    DateTime endDate = DateTime.UtcNow;
//    var (firstLine, secondLine) = _spaceTrack.GetTLEData(noradId, startDate, endDate);
//    if (!string.IsNullOrEmpty(firstLine) && !string.IsNullOrEmpty(secondLine))
//    {
//        var tle = new TLE
//        {
//            NoradId = int.Parse(noradId),
//            FirstLine = firstLine,
//            SecondLine = secondLine,
//            Timestamp = DateTime.UtcNow
//        };
//        _context.TLEs.InsertOne(tle); // Assuming _context.TLEs is the MongoDB collection
//    }
//}//////////////////////////////////////////////////////change 2

///////////////////////////////////////////////////////////change 1
//public async Task SaveTLEOfISSAsync()
//    {
//        string noradId = "25544"; // NORAD ID of ISS
//        DateTime startDate = DateTime.UtcNow.AddDays(-7); // Fetch recent TLE
//        DateTime endDate = DateTime.UtcNow;

//        string tleData = _spaceTrack.GetTLEData(noradId, startDate, endDate);

//        if (!string.IsNullOrEmpty(tleData))
//        {
//            var tle = new TLE
//            {
//                NoradId = int.Parse(noradId),
//                TleData = tleData,
//                Timestamp = DateTime.UtcNow
//            };

//            _context.TLEs.InsertOne(tle);
//        }
//    }
///////////////////////////////////////////////////////////////////////////////////////////////cahnge 1
//public class TLEService : ITLEService
//{
//private readonly MongoDbContext _context;

//public TLEService(MongoDbContext context)
//{
//    _context = context;
//}

//public async Task<string> FetchTLEFromSpaceTrack(int[] noradIds)
//{
//    string username = "mohammedsalah888m@gmail.com";  // Replace with your Space-Track username
//    string password = "space45track73mohamed5677salah";  // Replace with your Space-Track password
//    string baseUrl = "https://www.space-track.org";
//    string loginEndpoint = "/ajaxauth/login";
//    string queryEndpoint = $"/basicspacedata/query/class/tle_latest/NORAD_CAT_ID/{string.Join(",", noradIds)}/orderby/NORAD_CAT_ID ASC/format/tle";
//    //string queryEndpoint = $"/basicspacedata/query/class/tle_latest/NORAD_CAT_ID/{string.Join(",", noradIds)}/orderby/EPOCH desc/limit/1";
//    //string queryEndpoint = $"/basicspacedata/query/class/tle_latest/NORAD_CAT_ID/{string.Join(",", noradIds)}/orderby/EPOCH desc/limit/1/format/json";

//    using (HttpClient client = new HttpClient())
//    {
//        // Login
//        var loginContent = new FormUrlEncodedContent(new[]
//        {
//            new KeyValuePair<string, string>("identity", username),
//            new KeyValuePair<string, string>("password", password)
//        });

//        var loginResponse = await client.PostAsync(baseUrl + loginEndpoint, loginContent);
//        if (!loginResponse.IsSuccessStatusCode)
//            throw new Exception("Space-Track login failed.");

//        // Fetch TLE
//        var tleResponse = await client.GetStringAsync(baseUrl + queryEndpoint);
//        return tleResponse;
//    }
//}

//public async Task SaveTLEToDatabase(string tleData)
//{
//    //var lines = tleData.Split('\n', StringSplitOptions.RemoveEmptyEntries);
//    //for (int i = 0; i < lines.Length; i += 2)
//    //{
//    //    if (lines[i].StartsWith("1 ") && lines[i + 1].StartsWith("2 "))
//    //    {
//    //        var tle = new TLE
//    //        {
//    //            NoradId = int.Parse(lines[i].Split(' ').Last()),
//    //            Line1 = lines[i],
//    //            Line2 = lines[i + 1],
//    //            RetrievedAt = DateTime.UtcNow,
//    //        };
//    //        await _context.TLEs.InsertOneAsync(tle);
//    //    }
//    //    else
//    //    {
//    //        Console.WriteLine("Invalid TLE data format detected.");
//    //    }
//    //}

//    var lines = tleData.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
//    for (int i = 0; i < lines.Length; i += 2)
//    {
//        var tle = new TLE
//        {
//            NoradId = int.Parse(lines[i].Split(' ').Last()),
//            Line1 = lines[i],
//            Line2 = lines[i + 1],
//            RetrievedAt = DateTime.UtcNow
//        };

//        await _context.TLEs.InsertOneAsync(tle);
//    }
//}

