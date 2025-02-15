using Asp.Net_Core_API.Extension;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;
using MongoDB.Driver;
using SpaceTrack.DAL;
using SpaceTrack.Services;
using System.Text;
var builder = WebApplication.CreateBuilder(args);
string mongoConnectionString = builder.Configuration.GetValue<string>("MongoDbSettings:ConnectionString");
string mongoDatabaseName = builder.Configuration.GetValue<string>("MongoDbSettings:DatabaseName");
// Register MongoDbContext directly using retrieved values
builder.Services.AddSingleton<MongoDbContext>(sp =>
{
    return new MongoDbContext(mongoConnectionString, mongoDatabaseName);
});


// Register EciService as IEciService
builder.Services.AddScoped<IEciService, EciService>();



// Add services to the container.
///////////////
builder.Services.AddControllers();
// Configure JWT authentication
builder.Services.AddCustomJwtAuth(builder.Configuration);
builder.Services.AddAuthorization();////////////////////////////
// Add services to the container.
builder.Services.AddSingleton<MongoDbContext>(sp =>
    new MongoDbContext("mongodb://localhost:27017", "SpaceTrack"));
builder.Services.AddSingleton<SpaceTrack.Services.SpaceTrackTLE>();/////////////////////////////////////new

builder.Services.AddScoped<IUserService, UserService>();
// Add services to the container.
// Add services to the container.
//builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<ITLEService, TLEService>();
builder.Services.AddScoped<ObjectNameUpdateService>();////////////////////////////////////////////////////////////////////////////////////ObjectName

builder.Services.AddScoped<TLEService>();//////////////////////////////////////////////For Timer

builder.Services.AddHostedService<TLEPayloadsScheduler>(); // Register the background scheduler//////////////////////////////////////////////For Timer
builder.Services.AddHostedService<TLEdebrisScheduler>();
builder.Services.AddHostedService<TLERocketScheduler>();
builder.Services.AddHostedService<TLEUnknownScheduler>();
// Add services to the container.
builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(mongoConnectionString));

// Register MongoDbContext and inject ConnectionString and DatabaseName
builder.Services.AddSingleton<MongoDbContext>(sp =>
{
    var mongoSettings = builder.Configuration.GetSection("MongoDbSettings");
    var connectionString = mongoSettings["ConnectionString"];
    var databaseName = mongoSettings["DatabaseName"];
    return new MongoDbContext(connectionString, databaseName);
});


builder.Services.AddControllers();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//builder.Services.AddCustomJwtAuth(builder.Configuration);
//builder.Services.AddAuthorization();
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

///////////////


//// Add services to the container.
//builder.Services.AddSingleton<MongoDbContext>();
//builder.Services.AddScoped<TLEService>();/////////////////////////









