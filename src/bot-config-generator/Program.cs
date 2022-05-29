using FluentFTP;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApiClient;

Console.WriteLine("Hello, World!");

var builder = Host.CreateDefaultBuilder();

builder.ConfigureAppConfiguration((hostContext, builder) =>
{
    builder.AddUserSecrets<Program>();
}).ConfigureServices((hostContext, services) =>
{
    services.AddLogging();
    services.AddMemoryCache();
    services.AddApplicationInsightsTelemetryWorkerService();

    services.AddRepositoryApiClient(options =>
    {
        options.ApimBaseUrl = hostContext.Configuration["apim-base-url"];
        options.ApimSubscriptionKey = hostContext.Configuration["apim-subscription-key"];
    });

    services.AddHostedService<ConfigGeneratorService>();
});

var app = builder.Build();

app.Start();

public class ConfigGeneratorService : IHostedService
{
    private readonly IRepositoryApiClient repositoryApiClient;
    private readonly IConfiguration configuration;

    public ConfigGeneratorService(IRepositoryApiClient repositoryApiClient, IConfiguration configuration)
    {
        this.repositoryApiClient = repositoryApiClient;
        this.configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();
        sb.AppendLine("$bots = @(");

        var mySqlConnection = configuration["mysql-connection"];

        var gameTypes = new GameType[] { GameType.CallOfDuty2, GameType.CallOfDuty4, GameType.CallOfDuty5 };
        var gameServerDtos = await repositoryApiClient.GameServers.GetGameServers(gameTypes, null, GameServerFilter.ShowOnPortalServerList, 0, 0, null);
        gameServerDtos = gameServerDtos?.Where(gs => !string.IsNullOrWhiteSpace(gs.RconPassword) && !string.IsNullOrWhiteSpace(gs.FtpHostname) && !string.IsNullOrWhiteSpace(gs.FtpUsername) && !string.IsNullOrWhiteSpace(gs.FtpPassword)).ToList();

        var outputDirectory = @"C:\Users\FraserMolyneux\OneDrive\Desktop\bot-cofigs";

        foreach (var gameServerDto in gameServerDtos)
        {
            sb.AppendLine("    @{");
            sb.AppendLine($"        Name = '{gameServerDto.Title}'");
            sb.AppendLine($"        TaskName = '{gameServerDto.GameType}_{gameServerDto.Id}'");
            sb.AppendLine($"        LogFileName = '{gameServerDto.GameType}_{gameServerDto.Id}'");
            sb.AppendLine("    }");

            Console.WriteLine($"Generating bot configs for '{gameServerDto.Title}'");

            var mainTemplate = File.ReadAllText(@"templates\gameType_serverId.ini");
            var scheduledTask = File.ReadAllText(@"templates\gameType_serverId.xml");
            var pluginPortal = File.ReadAllText(@"templates\plugin_portal_gameType_serverId.ini");

            if (!string.IsNullOrWhiteSpace(gameServerDto.FtpHostname))
            {
                FtpClient? ftpClient = null;
                try
                {
                    ftpClient = new FtpClient(gameServerDto.FtpHostname, gameServerDto.FtpPort, gameServerDto.FtpUsername, gameServerDto.FtpPassword);
                    ftpClient.ValidateAnyCertificate = true;
                    ftpClient.AutoConnect();

                    ftpClient.SetWorkingDirectory(gameServerDto.LiveMod);

                    var files = ftpClient.GetListing();

                    var active = files.Where(f => f.Name.Contains(".log") && !f.Name.Contains("console")).OrderByDescending(f => f.Modified).FirstOrDefault();
                    if (active != null)
                    {
                        Console.WriteLine($"Autodetected that {gameServerDto.Title} is using log file: {active.FullName}");
                        mainTemplate = mainTemplate.Replace("__GAME_LOG__", $"ftp://{gameServerDto.FtpUsername}:{gameServerDto.FtpPassword}@{gameServerDto.FtpHostname}:{gameServerDto.FtpPort}{active.FullName}");
                    }
                }
                catch (Exception ex)
                {
                    continue;
                }
                finally
                {
                    ftpClient?.Dispose();
                }
            }
            else
            {
                mainTemplate = mainTemplate.Replace("__GAME_LOG__", @$"D:\TCAUsers\..\{gameServerDto.LiveMod}");
            }

            mainTemplate = mainTemplate.Replace("__MYSQL_CONNECTION__", mySqlConnection);
            mainTemplate = mainTemplate.Replace("__SERVER_ID__", gameServerDto.Id.ToString());
            mainTemplate = mainTemplate.Replace("__GAME_TYPE__", gameServerDto.GameType.ToString());
            scheduledTask = scheduledTask.Replace("__SERVER_ID__", gameServerDto.Id.ToString());
            scheduledTask = scheduledTask.Replace("__GAME_TYPE__", gameServerDto.GameType.ToString());
            pluginPortal = pluginPortal.Replace("__SERVER_ID__", gameServerDto.Id.ToString());
            mainTemplate = mainTemplate.Replace("__RCON_PASSWORD__", gameServerDto.RconPassword);
            mainTemplate = mainTemplate.Replace("__QUERY_PORT__", gameServerDto.QueryPort.ToString());
            mainTemplate = mainTemplate.Replace("__IP_ADDRESS__", gameServerDto.Hostname);
            pluginPortal = pluginPortal.Replace("__GAME_TYPE__", gameServerDto.GameType.ToString());

            switch (gameServerDto.GameType)
            {
                case GameType.CallOfDuty2:
                    mainTemplate = mainTemplate.Replace("__GAMETYPE_SHORT__", "cod2");
                    break;
                case GameType.CallOfDuty4:
                    mainTemplate = mainTemplate.Replace("__GAMETYPE_SHORT__", "cod4");
                    break;
                case GameType.CallOfDuty5:
                    mainTemplate = mainTemplate.Replace("__GAMETYPE_SHORT__", "cod5");
                    break;
            }

            var outputMainFilePath = Path.Join(outputDirectory, $"{gameServerDto.GameType}_{gameServerDto.Id}.ini");
            var outputScheduledTask = Path.Join(outputDirectory, $"{gameServerDto.GameType}_{gameServerDto.Id}.xml");
            var outputPlugin = Path.Join(outputDirectory, $"plugin_portal_{gameServerDto.GameType}_{gameServerDto.Id}.ini");

            if (File.Exists(outputMainFilePath)) File.Delete(outputMainFilePath);
            if (File.Exists(outputScheduledTask)) File.Delete(outputScheduledTask);
            if (File.Exists(outputPlugin)) File.Delete(outputPlugin);

            File.WriteAllText(outputMainFilePath, mainTemplate);
            File.WriteAllText(outputScheduledTask, scheduledTask);
            File.WriteAllText(outputPlugin, pluginPortal);
        }

        sb.AppendLine(")");

        var monitorOutputPath = Path.Join(outputDirectory, $"monitor.txt");
        if (File.Exists(monitorOutputPath)) File.Delete(monitorOutputPath);

        File.WriteAllText(monitorOutputPath, sb.ToString());
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
}