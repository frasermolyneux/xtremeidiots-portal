using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using XtremeIdiots.Portal.EventsApi.Abstractions.Models;
using XtremeIdiots.Portal.FuncHelpers.Providers;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Extensions;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApiClient;

namespace XtremeIdiots.Portal.IngestFunc;

public class PlayerEventsIngest
{
    private readonly ILogger<PlayerEventsIngest> _log;
    private readonly IRepositoryApiClient _repositoryApiClient;
    private readonly IMemoryCache memoryCache;
    private readonly IRepositoryTokenProvider _repositoryTokenProvider;

    public PlayerEventsIngest(
        ILogger<PlayerEventsIngest> log,
        IRepositoryTokenProvider repositoryTokenProvider,
        IRepositoryApiClient repositoryApiClient,
        IMemoryCache memoryCache)
    {
        _log = log;
        _repositoryTokenProvider = repositoryTokenProvider;
        _repositoryApiClient = repositoryApiClient;
        this.memoryCache = memoryCache;
    }

    [FunctionName("ProcessOnPlayerConnected")]
    public async Task ProcessOnPlayerConnected(
        [ServiceBusTrigger("player_connected_queue", Connection = "service-bus-connection-string")]
        string myQueueItem)
    {
        OnPlayerConnected onPlayerConnected;
        try
        {
            onPlayerConnected = JsonConvert.DeserializeObject<OnPlayerConnected>(myQueueItem);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "OnPlayerConnected was not in expected format");
            throw;
        }

        if (onPlayerConnected == null)
            throw new Exception("OnPlayerConnected event was null");

        if (string.IsNullOrWhiteSpace(onPlayerConnected.GameType))
            throw new Exception("OnPlayerConnected event contained null or empty 'GameType'");

        if (string.IsNullOrWhiteSpace(onPlayerConnected.Guid))
            throw new Exception("OnPlayerConnected event contained null or empty 'Guid'");

        if (!Enum.TryParse(onPlayerConnected.GameType, out GameType gameType))
            throw new Exception("OnPlayerConnected event contained invalid 'GameType'");

        _log.LogInformation(
            $"ProcessOnPlayerConnected :: Username: '{onPlayerConnected.Username}', Guid: '{onPlayerConnected.Guid}'");

        var accessToken = await _repositoryTokenProvider.GetRepositoryAccessToken();
        var existingPlayer = await _repositoryApiClient.Players.GetPlayerByGameType(accessToken, gameType, onPlayerConnected.Guid);

        if (existingPlayer == null)
        {
            var player = new PlayerDto
            {
                GameType = onPlayerConnected.GameType.ToGameType(),
                Guid = onPlayerConnected.Guid,
                Username = onPlayerConnected.Username,
                IpAddress = onPlayerConnected.IpAddress
            };

            await _repositoryApiClient.Players.CreatePlayer(accessToken, player);
        }
        else
        {
            if (onPlayerConnected.EventGeneratedUtc > existingPlayer.LastSeen)
            {
                existingPlayer.Username = onPlayerConnected.Username;
                existingPlayer.IpAddress = onPlayerConnected.IpAddress;

                await _repositoryApiClient.Players.UpdatePlayer(accessToken, existingPlayer);
            }
        }
    }

    [FunctionName("ProcessOnChatMessage")]
    public async Task ProcessOnChatMessage(
        [ServiceBusTrigger("chat_message_queue", Connection = "service-bus-connection-string")]
        string myQueueItem)
    {
        OnChatMessage onChatMessage;
        try
        {
            onChatMessage = JsonConvert.DeserializeObject<OnChatMessage>(myQueueItem);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "OnChatMessage was not in expected format");
            throw;
        }

        if (onChatMessage == null)
            throw new Exception("OnChatMessage event was null");

        if (string.IsNullOrWhiteSpace(onChatMessage.GameType))
            throw new Exception("OnChatMessage event contained null or empty 'GameType'");

        if (string.IsNullOrWhiteSpace(onChatMessage.Guid))
            throw new Exception("OnChatMessage event contained null or empty 'Guid'");

        if (!Enum.TryParse(onChatMessage.GameType, out GameType gameType))
            throw new Exception("OnChatMessage event contained invalid 'GameType'");

        _log.LogInformation($"ProcessOnChatMessage :: Username: '{onChatMessage.Username}', Guid: '{onChatMessage.Guid}', Message: '{onChatMessage.Message}', Timestamp: '{onChatMessage.EventGeneratedUtc}'");

        var playerId = await GetPlayerId(gameType, onChatMessage.Guid);

        if (playerId != Guid.Empty)
        {
            var chatMessage = new ChatMessageDto
            {
                GameServerId = (Guid)onChatMessage.ServerId,
                PlayerId = playerId,
                Username = onChatMessage.Username,
                Message = onChatMessage.Message,
                Type = onChatMessage.Type.ToChatType(),
                Timestamp = onChatMessage.EventGeneratedUtc
            };

            var accessToken = await _repositoryTokenProvider.GetRepositoryAccessToken();
            await _repositoryApiClient.ChatMessages.CreateChatMessage(accessToken, chatMessage);
        }
        else
        {
            _log.LogWarning($"ProcessOnChatMessage :: NOPLAYER :: Username: '{onChatMessage.Username}', Guid: '{onChatMessage.Guid}', Message: '{onChatMessage.Message}', Timestamp: '{onChatMessage.EventGeneratedUtc}'");
        }
    }

    [FunctionName("ProcessOnMapVote")]
    public async Task ProcessOnMapVote(
     [ServiceBusTrigger("map_vote_queue", Connection = "service-bus-connection-string")]
        string myQueueItem)
    {
        OnMapVote onMapVote;
        try
        {
            onMapVote = JsonConvert.DeserializeObject<OnMapVote>(myQueueItem);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "OnMapVote was not in expected format");
            throw;
        }

        if (onMapVote == null)
            throw new Exception("OnMapVote event was null");

        if (string.IsNullOrWhiteSpace(onMapVote.MapName))
            throw new Exception("OnMapVote event contained null or empty 'MapName'");

        if (string.IsNullOrWhiteSpace(onMapVote.Guid))
            throw new Exception("OnMapVote event contained null or empty 'Guid'");

        if (onMapVote.Like == null)
            throw new Exception("OnMapVote event contained null 'Like'");

        if (!Enum.TryParse(onMapVote.GameType, out GameType gameType))
            throw new Exception("OnMapVote event contained invalid 'GameType'");

        _log.LogInformation($"ProcessOnMapVote ::  Guid: '{onMapVote.Guid}', Map Name: '{onMapVote.MapName}', Like: '{onMapVote.Like}'");

        var playerId = await GetPlayerId(gameType, onMapVote.Guid);

        if (playerId != Guid.Empty)
        {
            var accessToken = await _repositoryTokenProvider.GetRepositoryAccessToken();
            var map = await _repositoryApiClient.Maps.GetMap(accessToken, gameType, onMapVote.MapName);

            await _repositoryApiClient.Maps.UpsertMapVote(accessToken, map.MapId, playerId, (bool)onMapVote.Like);
        }
        else
        {
            _log.LogWarning($"ProcessOnMapVote :: NOPLAYER :: Guid: '{onMapVote.Guid}', Map Name: '{onMapVote.MapName}', Like: '{onMapVote.Like}'");
        }
    }

    private async Task<Guid> GetPlayerId(GameType gameType, string guid)
    {
        var cacheKey = $"{gameType}-${guid}";

        if (!memoryCache.TryGetValue(cacheKey, out Guid playerId))
        {
            var accessToken = await _repositoryTokenProvider.GetRepositoryAccessToken();

            var player = await _repositoryApiClient.Players.GetPlayerByGameType(accessToken, gameType, guid);

            if (player != null)
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(15));
                memoryCache.Set(cacheKey, player.Id, cacheEntryOptions);

                return playerId;
            }
            else
            {
                return Guid.Empty;
            }
        }

        return playerId;
    }
}