using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SGPdotNET.TLE;
using SGPdotNET.Propagation;
using SpaceTrack.DAL.Model;
using SpaceTrack.Services;


namespace SpaceTrack.Services
{
    public class EciService : IEciService
    {

        private readonly IMongoCollection<EciPosition> _eciCollection;
        private readonly ILogger<EciService> _logger;

        public EciService(IMongoClient mongoClient, ILogger<EciService> logger)
        {
            if (mongoClient == null) throw new ArgumentNullException(nameof(mongoClient));

            var database = mongoClient.GetDatabase("SpaceTrack");
            _eciCollection = database.GetCollection<EciPosition>("EciPositions");

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Ensure Index on NoradId for faster queries
            var indexKeysDefinition = Builders<EciPosition>.IndexKeys.Ascending(e => e.NoradId);
            _eciCollection.Indexes.CreateOne(new CreateIndexModel<EciPosition>(indexKeysDefinition));
        }

        public Task<EciPosition> ComputeEciAsync(string? Id, int NoradId, string FirstLine, string SecondLine, DateTime Timestamp)
        {
            throw new NotImplementedException();
        }

        public async Task<List<EciPosition>> ComputeEciForDayAsync(string objectId, int noradId, string firstLine, string secondLine, DateTime date)
        {
            if (string.IsNullOrWhiteSpace(objectId))
                throw new ArgumentException("Object ID is required.", nameof(objectId));
            if (noradId <= 0)
                throw new ArgumentException("Norad ID must be a positive integer.", nameof(noradId));
            if (string.IsNullOrWhiteSpace(firstLine) || string.IsNullOrWhiteSpace(secondLine))
                throw new ArgumentException("Both TLE lines are required.", nameof(firstLine));

            try
            {
                var tle = new Tle(firstLine, secondLine);
                var propagator = new Sgp4(tle);
                var eciPositions = new List<EciPosition>();

                DateTime startTime = date.Date;
                DateTime endTime = startTime.AddDays(1);
                TimeSpan timeStep = TimeSpan.FromMinutes(1);

                for (DateTime utcTime = startTime; utcTime < endTime; utcTime += timeStep)
                {
                    double minutesSinceEpoch = (utcTime - tle.Epoch).TotalMinutes;
                    var result = propagator.FindPosition(minutesSinceEpoch);

                    if (result == null || result.Position == null || result.Velocity == null)
                    {
                        _logger.LogWarning("SGP4 failed at {UtcTime} for Object ID: {ObjectId}", utcTime, objectId);
                        continue;
                    }

                    var eciPosition = new EciPosition
                    {
                        ObjectId = objectId,
                        NoradId = noradId,
                        UtcTime = utcTime,
                        FirstLine = firstLine,
                        SecondLine = secondLine,
                        X = result.Position.X,
                        Y = result.Position.Y,
                        Z = result.Position.Z,
                        Vx = result.Velocity.X,
                        Vy = result.Velocity.Y,
                        Vz = result.Velocity.Z
                    };

                    eciPositions.Add(eciPosition);
                }

                if (eciPositions.Any())
                {
                    // Check for existing records before inserting**
                    var existingRecords = await _eciCollection.Find(e => e.NoradId == noradId && e.UtcTime >= startTime && e.UtcTime < endTime).ToListAsync();

                    if (existingRecords.Any())
                    {
                        // Update existing records**
                        foreach (var position in eciPositions)
                        {
                            var filter = Builders<EciPosition>.Filter.Where(e => e.NoradId == noradId && e.UtcTime == position.UtcTime);
                            var update = Builders<EciPosition>.Update
                                .Set(e => e.X, position.X)
                                .Set(e => e.Y, position.Y)
                                .Set(e => e.Z, position.Z)
                                .Set(e => e.Vx, position.Vx)
                                .Set(e => e.Vy, position.Vy)
                                .Set(e => e.Vz, position.Vz)
                                .Set(e => e.FirstLine, position.FirstLine)
                                .Set(e => e.SecondLine, position.SecondLine);

                            await _eciCollection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
                        }

                        _logger.LogInformation("Updated existing ECI positions for {ObjectId} on {Date}", objectId, date);
                    }
                    else
                    {
                        // Insert new records if no existing ones are found
                        await _eciCollection.InsertManyAsync(eciPositions);
                        _logger.LogInformation("Inserted {Count} new ECI positions for {ObjectId} on {Date}", eciPositions.Count, objectId, date);
                    }
                }

                return eciPositions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error computing daily ECI positions for Object ID: {ObjectId}", objectId);
                throw;
            }
        }

    }
}


    







