using MongoDB.Driver;
using SpaceTrack.DAL.Model;
using SpaceTrack.DAL;

namespace SpaceTrack.Services
{
    public class ObjectNameUpdateService
    {
        private readonly MongoDbContext _context;
        private readonly SpaceTrackTLE _spaceTrack;

        public ObjectNameUpdateService(MongoDbContext context)
        {
            _context = context;
            _spaceTrack = new SpaceTrackTLE();
        }

        public async Task UpdateObjectNamesAsync()
        {
            const int batchSize = 50; // Process 50 records at a time
            int skip = 0;
            var totalCount = _context.TLEPAYLOADS.CountDocuments(FilterDefinition<TLEPayloads>.Empty);

            while (skip < totalCount)
            {
                var batchPayloads = _context.TLEPAYLOADS
                    .Find(FilterDefinition<TLEPayloads>.Empty)
                    .Skip(skip)
                    .Limit(batchSize)
                    .ToList();

                foreach (var payload in batchPayloads)
                {
                    try
                    {
                        // Fetch updated name with retry
                        string updatedName = await GetPayloadNameWithRetryAsync(payload.NoradId);

                        // Check if the name needs to be updated
                        if (!string.IsNullOrEmpty(updatedName) && (payload.ObjectName == null || updatedName != payload.ObjectName))
                        {
                            var updateDefinition = Builders<TLEPayloads>.Update
                                .Set(p => p.ObjectName, updatedName)
                                .Set(p => p.Timestamp, DateTime.UtcNow);

                            await _context.TLEPAYLOADS.UpdateOneAsync(
                                p => p.NoradId == payload.NoradId,
                                updateDefinition
                            );

                            Console.WriteLine($"Updated NORAD ID {payload.NoradId} with name {updatedName}.");
                        }

                        // Add a delay to avoid throttling
                        await Task.Delay(500);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error updating NORAD ID {payload.NoradId}: {ex.Message}");
                    }
                }

                skip += batchSize; // Move to the next batch
            }
        }

        private async Task<string> GetPayloadNameWithRetryAsync(int noradId, int maxRetries = 3)
        {
            int retryCount = 0;
            while (retryCount < maxRetries)
            {
                try
                {
                    return await _spaceTrack.GetPayloadNameAsync(noradId);
                }
                catch (Exception ex)
                {
                    retryCount++;
                    Console.WriteLine($"Retry {retryCount}/{maxRetries} for NORAD ID {noradId}: {ex.Message}");

                    if (retryCount >= maxRetries)
                    {
                        throw; // Rethrow after max retries
                    }

                    await Task.Delay(1000); // Wait before retrying
                }
            }

            return null; // Return null if all retries fail
        }
    }
}

/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using SpaceTrack.DAL.Model;
using SpaceTrack.DAL;

namespace SpaceTrack.Services
{
    public class ObjectNameUpdateService
    {
        private readonly MongoDbContext _context;
        private readonly SpaceTrackTLE _spaceTrack;

        public ObjectNameUpdateService(MongoDbContext context)
        {
            _context = context;
            _spaceTrack = new SpaceTrackTLE();
        }

        public async Task UpdateObjectNamesAsync()
        {
            // Fetch all existing TLE payloads from the database
            var allPayloads = _context.TLEPAYLOADS.Find(FilterDefinition<TLEPayloads>.Empty).ToList();

            foreach (var payload in allPayloads)
            {
                try
                {
                    // Fetch the latest OBJECT_NAME using the NORAD ID
                    string updatedName = await _spaceTrack.GetPayloadNameAsync(payload.NoradId);

                    if (!string.IsNullOrEmpty(updatedName) && updatedName != payload.ObjectName)
                    {
                        // Update the ObjectName in the database
                        var updateDefinition = Builders<TLEPayloads>.Update
                            .Set(p => p.ObjectName, updatedName)
                            .Set(p => p.Timestamp, DateTime.UtcNow); // Optional: update timestamp

                        await _context.TLEPAYLOADS.UpdateOneAsync(
                            p => p.NoradId == payload.NoradId,
                            updateDefinition
                        );

                        Console.WriteLine($"Updated NORAD ID {payload.NoradId} with name {updatedName}.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating NORAD ID {payload.NoradId}: {ex.Message}");
                }
            }
        }
    }
}
*/