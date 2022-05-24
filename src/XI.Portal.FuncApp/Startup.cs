using FM.GeoLocation.Client.Extensions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Reflection;
using XI.Portal.FuncApp;
using XI.Portal.Players.Extensions;
using XI.Utilities.FtpHelper;
using XtremeIdiots.Portal.InvisionApiClient;
using XtremeIdiots.Portal.RepositoryApiClient;

[assembly: FunctionsStartup(typeof(Startup))]

namespace XI.Portal.FuncApp
{
    public class Startup : FunctionsStartup
    {
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            var context = builder.GetContext();

            builder.ConfigurationBuilder
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, "appsettings.json"), true, false)
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, $"appsettings.{context.EnvironmentName}.json"),
                    true, false)
                .AddJsonFile("local.settings.json", true, false)
                .AddUserSecrets(Assembly.GetExecutingAssembly(), false)
                .AddEnvironmentVariables();

            base.ConfigureAppConfiguration(builder);
        }


        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = builder.GetContext().Configuration;

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

            builder.Services.AddRepositoryApiClient(options =>
            {
                options.ApimBaseUrl = "https://apim-portal-prd-uksouth-01.azure-api.net";
                options.ApimSubscriptionKey = config["apimsubscriptionkey"];
            });

            builder.Services.AddPlayersModule(options =>
            {
                options.ConfigureBanFilesRepositoryOptions(repositoryOptions =>
                {
                    repositoryOptions.StorageConnectionString = config["AppDataContainer:StorageConnectionString"];
                    repositoryOptions.StorageContainerName = config["BanFilesRepository:StorageContainerName"];
                });
            });

            builder.Services.AddInvisionApiClient(options =>
            {
                options.BaseUrl = config["XtremeIdiotsForums:BaseUrl"];
                options.ApiKey = config["XtremeIdiotsForums:ApiKey"];
            });

            builder.Services.AddSingleton<IFtpHelper, FtpHelper>();

            builder.Services.AddMemoryCache();
        }
    }
}