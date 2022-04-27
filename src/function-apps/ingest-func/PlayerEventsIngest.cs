using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using XtremeIdiots.Portal.CommonLib.Events;
using XtremeIdiots.Portal.CommonLib.Models;
using XtremeIdiots.Portal.FuncHelpers.Providers;
using XtremeIdiots.Portal.RepositoryApiClient;

namespace XtremeIdiots.Portal.IngestFunc;

public class PlayerEventsIngest
{
    private readonly ILogger<PlayerEventsIngest> _log;
    private readonly IRepositoryApiClient _repositoryApiClient;
    private readonly IRepositoryTokenProvider _repositoryTokenProvider;

    public PlayerEventsIngest(
        ILogger<PlayerEventsIngest> log,
        IRepositoryTokenProvider repositoryTokenProvider,
        IRepositoryApiClient repositoryApiClient)
    {
        _log = log;
        _repositoryTokenProvider = repositoryTokenProvider;
        _repositoryApiClient = repositoryApiClient;
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

        _log.LogInformation(
            $"ProcessOnPlayerConnected :: Username: '{onPlayerConnected.Username}', Guid: '{onPlayerConnected.Guid}'");

        var accessToken = await _repositoryTokenProvider.GetRepositoryAccessToken();
        var existingPlayer =
            await _repositoryApiClient.Players.GetPlayerByGameType(accessToken, onPlayerConnected.GameType,
                onPlayerConnected.Guid);

        if (existingPlayer == null)
        {
            var player = new PlayerApiDto
            {
                GameType = onPlayerConnected.GameType,
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

        _log.LogInformation(
            $"ProcessOnChatMessage :: Username: '{onChatMessage.Username}', Guid: '{onChatMessage.Guid}', Message: '{onChatMessage.Message}', Timestamp: '{onChatMessage.EventGeneratedUtc}'");

        var accessToken = await _repositoryTokenProvider.GetRepositoryAccessToken();

        var player =
            await _repositoryApiClient.Players.GetPlayerByGameType(accessToken, onChatMessage.GameType,
                onChatMessage.Guid);

        if (player != null)
        {
            var chatMessage = new ChatMessageApiDto
            {
                GameServerId = onChatMessage.ServerId,
                PlayerId = player.Id,
                Username = onChatMessage.Username,
                Message = onChatMessage.Message,
                Type = onChatMessage.Type,
                Timestamp = onChatMessage.EventGeneratedUtc
            };

            await _repositoryApiClient.ChatMessages.CreateChatMessage(accessToken, chatMessage);
        }
    }
}