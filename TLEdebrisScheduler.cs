﻿using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SpaceTrack.DAL;
using SpaceTrack.DAL.Model;

namespace SpaceTrack.Services
{
    public class TLEdebrisScheduler : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TLEdebrisScheduler> _logger;

        public TLEdebrisScheduler(IServiceProvider serviceProvider , ILogger<TLEdebrisScheduler> logger)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow; // Use UTC for consistency
                var targetTime = new DateTime(now.Year, now.Month, now.Day, 3, 1, 0, DateTimeKind.Utc); // 7:30 AM UTC

                if (now > targetTime)
                {
                    targetTime = targetTime.AddDays(1); // Schedule for the next day if the time has passed
                }

                var delay = targetTime - now;
                await Task.Delay(delay, stoppingToken); // Wait until the scheduled time

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var tleService = scope.ServiceProvider.GetRequiredService<TLEService>();
                    var eciService = scope.ServiceProvider.GetRequiredService<IEciService>();





                    await tleService.SaveOrUpdateAllDebrisTLEsAsync(); // Call the method
                    // Fetch latest TLEs from DB
                     var tleContext = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
                    var latestTLEs = tleContext.TLEs.Find(Builders<TLEDebris>.Filter.Empty).ToList();


                    foreach (var tle in latestTLEs)
                    {
                        await eciService.ComputeEciAsync(null, tle.NoradId, tle.FirstLine, tle.SecondLine, tle.Timestamp);
                    }

                    _logger.LogInformation("TLE and ECI updates completed successfully.");
                


                }
                catch (Exception ex)
                {
                    // Log errors (optional: integrate a logging framework)
                    Console.WriteLine($"Error in scheduled task: {ex.Message}");
                }
            }
        }
    }
}
