using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using XtremeIdiots.Portal.EventsApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers;
using XtremeIdiots.Portal.RepositoryApiClient;

namespace XtremeIdiots.Portal.IngestFunc;

public class ServerEventsIngest
{
    private readonly ILogger<ServerEventsIngest> _log;
    private readonly IRepositoryApiClient _repositoryApiClient;

    public ServerEventsIngest(
        ILogger<ServerEventsIngest> log,
        IRepositoryApiClient repositoryApiClient)
    {
        _log = log;
        _repositoryApiClient = repositoryApiClient;
    }

    [FunctionName("ProcessOnServerConnected")]
    public async Task ProcessOnServerConnected(
        [ServiceBusTrigger("server_connected_queue", Connection = "service-bus-connection-string")]
        string myQueueItem)
    {
        OnServerConnected? onServerConnected;
        try
        {
            onServerConnected = JsonConvert.DeserializeObject<OnServerConnected>(myQueueItem);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "OnServerConnected was not in expected format");
            throw;
        }

        if (onServerConnected == null)
            throw new Exception("OnServerConnected event was null");

        if (!Guid.TryParse(onServerConnected.Id, out Guid gameServerId))
            throw new Exception("OnServerConnected event contained null or empty 'onServerConnected'");

        _log.LogInformation(
            $"OnServerConnected :: Id: '{onServerConnected.Id}', GameType: '{onServerConnected.GameType}'");

        var gameServerEvent = new CreateGameServerEventDto(gameServerId, "OnServerConnected", JsonConvert.SerializeObject(onServerConnected));
        await _repositoryApiClient.GameServersEvents.CreateGameServerEvent(gameServerEvent);
    }

    [FunctionName("ProcessOnMapChange")]
    public async Task ProcessOnMapChange(
        [ServiceBusTrigger("map_change_queue", Connection = "service-bus-connection-string")]
        string myQueueItem)
    {
        OnMapChange? onMapChange;
        try
        {
            onMapChange = JsonConvert.DeserializeObject<OnMapChange>(myQueueItem);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "OnMapChange was not in expected format");
            throw;
        }

        if (onMapChange == null)
            throw new Exception("OnMapChange event was null");

        if (onMapChange.ServerId == null)
            throw new Exception("OnMapChange event contained null or empty 'ServerId'");

        _log.LogInformation(
            $"ProcessOnMapChange :: GameName: '{onMapChange.GameName}', GameType: '{onMapChange.GameType}', MapName: '{onMapChange.MapName}'");

        var gameServerEvent = new CreateGameServerEventDto((Guid)onMapChange.ServerId, "MapChange", JsonConvert.SerializeObject(onMapChange));
        await _repositoryApiClient.GameServersEvents.CreateGameServerEvent(gameServerEvent);
    }
}