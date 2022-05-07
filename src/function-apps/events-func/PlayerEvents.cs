using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using XtremeIdiots.Portal.EventsApi.Abstractions.Models;

namespace XtremeIdiots.Portal.EventsFunc;

public class PlayerEvents
{
    [FunctionName("OnPlayerConnected")]
    [return: ServiceBus("player_connected_queue", Connection = "service-bus-connection-string")]
    public string OnPlayerConnected([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] string input,
        ILogger log)
    {
        OnPlayerConnected onPlayerConnected;
        try
        {
            onPlayerConnected = JsonConvert.DeserializeObject<OnPlayerConnected>(input);
        }
        catch (Exception ex)
        {
            log.LogError($"OnPlayerConnected Raw Input: '{input}'");
            log.LogError(ex, "OnPlayerConnected was not in expected format");
            throw;
        }

        return JsonConvert.SerializeObject(onPlayerConnected);
    }

    [FunctionName("OnChatMessage")]
    [return: ServiceBus("chat_message_queue", Connection = "service-bus-connection-string")]
    public static string OnChatMessage([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] string input,
        ILogger log)
    {
        OnChatMessage onChatMessage;
        try
        {
            onChatMessage = JsonConvert.DeserializeObject<OnChatMessage>(input);
        }
        catch (Exception ex)
        {
            log.LogError($"OnChatMessage Raw Input: '{input}'");
            log.LogError(ex, "OnChatMessage was not in expected format");
            throw;
        }

        return JsonConvert.SerializeObject(onChatMessage);
    }
}