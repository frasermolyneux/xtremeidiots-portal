﻿using System;
using System.Reflection;
using FM.GeoLocation.Client.Extensions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using XI.Forums.Extensions;
using XI.Portal.Data.Legacy;
using XI.Portal.FuncApp;
using XI.Portal.Maps.Extensions;
using XI.Portal.Players.Extensions;
using XI.Portal.Servers.Extensions;
using XI.Portal.Servers.Integrations.Extensions;
using XI.Utilities.FtpHelper;
using XI.AzureTableLogging.Extensions;
using XI.AzureTableLogging.Interfaces;

[assembly: FunctionsStartup(typeof(Startup))]

namespace XI.Portal.FuncApp
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var basePath = IsDevelopmentEnvironment() ? Environment.GetEnvironmentVariable("AzureWebJobsScriptRoot") : $"{Environment.GetEnvironmentVariable("HOME")}\\site\\wwwroot";

            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", false, false)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT")}.json", true, false)
                .AddJsonFile("local.settings.json", true, false)
                .AddUserSecrets(Assembly.GetExecutingAssembly(), false)
                .AddEnvironmentVariables()
                .Build();

            builder.Services.AddSingleton<IConfiguration>(config);

            builder.Services.AddDbContext<LegacyPortalContext>(options =>
                options.UseSqlServer(config["LegacyPortalContext:ConnectionString"]));

            Action<IAzureTableLoggerOptions> azureTableLoggerOptions = options =>
            {
                options.CreateTableIfNotExists = true;
                options.StorageTableName = config["AzureTableLogger:StorageTableName"];
                options.StorageConnectionString = config["AppDataContainer:StorageConnectionString"];
            };

            builder.Services.AddLogging(logging =>
            {
                logging.AddAzureTableLogger(azureTableLoggerOptions);
            });

            builder.Services.AddDirectAzureTableLogger(azureTableLoggerOptions);

            builder.Services.AddGeoLocationClient(options =>
            {
                options.BaseUrl = config["GeoLocation:BaseUrl"];
                options.ApiKey = config["GeoLocation:ApiKey"];
                options.UseMemoryCache = true;
                options.BubbleExceptions = false;
                options.CacheEntryLifeInMinutes = 60;
                options.RetryTimespans = new[]
                {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(3),
                    TimeSpan.FromSeconds(5)
                };
            });

            builder.Services.AddServersModule(options =>
            {
                options.ConfigureGameServerStatusRepository(repositoryOptions =>
                {
                    repositoryOptions.StorageConnectionString = config["AppDataContainer:StorageConnectionString"];
                    repositoryOptions.StorageTableName = config["GameServerStatusRepository:StorageTableName"];
                });

                options.ConfigureGameServerStatusStatsRepository(repositoryOptions =>
                {
                    repositoryOptions.StorageConnectionString = config["AppDataContainer:StorageConnectionString"];
                    repositoryOptions.StorageTableName = config["GameServerStatusStatsRepository:StorageTableName"];
                });

                options.ConfigureLogFileMonitorStateRepository(repositoryOptions =>
                {
                    repositoryOptions.StorageConnectionString = config["AppDataContainer:StorageConnectionString"];
                    repositoryOptions.StorageTableName = config["LogFileMonitorState:StorageTableName"];
                });
            });

            builder.Services.AddPlayersModule(options =>
            {
                options.ConfigurePlayerLocationsRepository(repositoryOptions =>
                {
                    repositoryOptions.StorageConnectionString = config["AppDataContainer:StorageConnectionString"];
                    repositoryOptions.StorageTableName = config["PlayerLocationsRepository:StorageTableName"];
                });

                options.ConfigurePlayersCacheRepository(repositoryOptions =>
                {
                    repositoryOptions.StorageConnectionString = config["AppDataContainer:StorageConnectionString"];
                    repositoryOptions.StorageTableName = config["PlayerCacheRepository:StorageTableName"];
                });

                options.ConfigureBanFilesRepositoryOptions(repositoryOptions =>
                {
                    repositoryOptions.StorageConnectionString = config["AppDataContainer:StorageConnectionString"];
                    repositoryOptions.StorageContainerName = config["BanFilesRepository:StorageContainerName"];
                });
            });

            builder.Services.AddMapsModule(options =>
            {
                options.ConfigureMapsRepository(repositoryOptions => { repositoryOptions.MapRedirectBaseUrl = config["MapsRedirect:BaseUrl"]; });

                options.ConfigureMapRedirectRepository(repositoryOptions =>
                {
                    repositoryOptions.MapRedirectBaseUrl = config["MapsRedirect:BaseUrl"];
                    repositoryOptions.ApiKey = config["MapsRedirect:ApiKey"];
                });

                options.ConfigureMapPopularityRepository(repositoryOptions =>
                {
                    repositoryOptions.StorageConnectionString = config["AppDataContainer:StorageConnectionString"];
                    repositoryOptions.StorageTableName = config["MapPopularity:StorageTableName"];
                });
            });

            builder.Services.AddForumsClient(options =>
            {
                options.BaseUrl = config["XtremeIdiotsForums:BaseUrl"];
                options.ApiKey = config["XtremeIdiotsForums:ApiKey"];
            });

            builder.Services.AddChatCommands();

            builder.Services.AddSingleton<IFtpHelper, FtpHelper>();
        }

        private bool IsDevelopmentEnvironment()
        {
            return "Development".Equals(Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT"), StringComparison.OrdinalIgnoreCase);
        }
    }
}