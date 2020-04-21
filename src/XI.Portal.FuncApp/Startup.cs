using System;
using System.Reflection;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using XI.AzureTableLogging.Extensions;
using XI.Portal.Data.Legacy;
using XI.Portal.FuncApp;
using XI.Portal.Players.Extensions;
using XI.Portal.Servers.Configuration;
using XI.Portal.Servers.Extensions;

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

            builder.Services.AddServersModule(options =>
            {
                options.ConfigureGameServersRepository(repositoryOptions => { });
                options.ConfigureBanFileMonitorsRepository(repositoryOptions => { });
                options.ConfigureFileMonitorsRepository(repositoryOptions => { });
                options.ConfigureRconMonitorsRepository(repositoryOptions => { });
                options.ConfigureGameServerStatusRepository(repositoryOptions =>
                {
                    repositoryOptions.StorageConnectionString = config["AppDataContainer:StorageConnectionString"];
                    repositoryOptions.StorageTableName = config["GameServerStatusRepository:StorageTableName"];
                    repositoryOptions.GeoLocationClientConfiguration = new GeoLocationClientConfig
                    {
                        BaseUrl = config["GeoLocation:BaseUrl"],
                        ApiKey = config["GeoLocation:ApiKey"]
                    };
                });
                options.ConfigureChatLogsRepository(repositoryOptions => { });
            });

            builder.Services.AddPlayersModule(options =>
            {
                options.ConfigurePlayersRepository(repositoryOptions => { });
                options.ConfigureAdminActionsRepository(repositoryOptions => { });
                options.ConfigurePlayerLocationsRepository(repositoryOptions =>
                {
                    repositoryOptions.StorageConnectionString = config["AppDataContainer:StorageConnectionString"];
                    repositoryOptions.StorageTableName = config["PlayerLocationsRepository:StorageTableName"];
                    repositoryOptions.GeoLocationClientConfiguration = new Players.Configuration.GeoLocationClientConfig
                    {
                        BaseUrl = config["GeoLocation:BaseUrl"],
                        ApiKey = config["GeoLocation:ApiKey"]
                    };
                });
            });

            builder.Services.AddLogging(
                logging =>
                {
                    logging.AddAzureTableLogger(options =>
                    {
                        options.CreateTableIfNotExists = true;
                        options.StorageTableName = config["Logging:AzureTableLogger:StorageTableName"];
                        options.StorageConnectionString = config["AppDataContainer:StorageConnectionString"];
                    });
                });
        }

        private bool IsDevelopmentEnvironment()
        {
            return "Development".Equals(Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT"), StringComparison.OrdinalIgnoreCase);
        }
    }
}