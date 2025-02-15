using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using SpaceTrack.DAL.Model;

namespace SpaceTrack.DAL
{
    ////public MongoDbContext(string connectionString, string databaseName)
    ////{
    //    //var client = new MongoClient(connectionString);
    //    //_database = client.GetDatabase(databaseName);

    //    // Ensure unique index on NoradId and Timestamp
    //    var indexModel = new CreateIndexModel<TLEPayloads>(
    //        Builders<TLEPayloads>.IndexKeys.Ascending(t => t.NoradId).Ascending(t => t.Timestamp),
    //        new CreateIndexOptions { Unique = true }
    //    );
    //    TLEPAYLOADS.Indexes.CreateOne(indexModel);
    //}

    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(string connectionString, string databaseName)
        {
            var client = new MongoClient("mongodb://localhost:27017");
            _database = client.GetDatabase("SpaceTrack");
           

        }
        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
        public IMongoCollection<Satellite> Satellites => _database.GetCollection<Satellite>("Satellites");
        public IMongoCollection<ResetPasswordRequest> ResetPasswordRequests => _database.GetCollection<ResetPasswordRequest>("ResetPasswordRequests");
        public IMongoCollection<ContactMessage> ContactMessages => _database.GetCollection<ContactMessage>("ContactMessages");
        public IMongoCollection<Foremail> Foremails => _database.GetCollection<Foremail>("Foremails");
        
        public IMongoCollection<TLEPayloads> TLEPAYLOADS => _database.GetCollection<TLEPayloads>("TLEPayloads");
        public IMongoCollection<EciPosition> EciPositions => _database.GetCollection<EciPosition>("EciPositions");











        public IMongoCollection<TLERockets> TLEROCKETS => _database.GetCollection<TLERockets>("TLERockets"); 
        public IMongoCollection<TLEUnknown> TLEUNKNOWN => _database.GetCollection<TLEUnknown>("TLEUnknown");

        public IMongoCollection<TLEDebris> TLEs => _database.GetCollection<TLEDebris>("TLEDebris");
    }
}




