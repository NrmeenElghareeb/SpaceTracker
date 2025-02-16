using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SpaceTrack.DAL;
using SpaceTrack.DAL.Model;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authorization;

namespace SpaceAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class CheckForSatelliteController : ControllerBase
    {
        private readonly IMongoCollection<Satellite> _satelliteCollection;

        // Constructor to inject MongoDbContext
        public CheckForSatelliteController()
        {
            var dbContext = new MongoDbContext("YourMongoDBConnectionString", "YourDatabaseName");
            _satelliteCollection = dbContext.Satellites;
        }

        // Endpoint 2: GET - Check if Satellite exists by NoardId or Name
        [Authorize(Roles = "user,admin")]
        [HttpGet("exists")]
        public IActionResult CheckSatellite([FromQuery] string noardId = null, [FromQuery] string name = null)
        {
            if (string.IsNullOrEmpty(noardId) && string.IsNullOrEmpty(name))
            {
                return BadRequest("Provide either NoardId or Name to check for satellite existence.");
            }

            // Query MongoDB
            var filter = Builders<Satellite>.Filter.Or(
                Builders<Satellite>.Filter.Eq(s => s.NoardId, noardId),
                Builders<Satellite>.Filter.Eq(s => s.Name, name)
            );

            var satellite = _satelliteCollection.Find(filter).FirstOrDefault();
            if (satellite != null)
            {
                return Ok($"Satellite exists: NoardId={satellite.NoardId}, Name={satellite.Name}");
            }
            else
            {
                return NotFound("Satellite not found.");
            }
        }
    }
}
