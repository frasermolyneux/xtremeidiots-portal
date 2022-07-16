using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using XtremeIdiots.Portal.EventsApi.Abstractions.Models;

namespace XtremeIdiots.Portal.EventsFunc;

public class ServerEvents
{
    [FunctionName("OnServerConnected")]
    [return: ServiceBus("server_connected_queue", Connection = "service_bus_connection_string")]
    public string OnServerConnected([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] string input,
        ILogger log)
    {
        OnServerConnected onServerConnected;
        try
        {
            onServerConnected = JsonConvert.DeserializeObject<OnServerConnected>(input);
        }
        catch (Exception ex)
        {
            log.LogError($"OnServerConnected Raw Input: '{input}'");
            log.LogError(ex, "OnServerConnected was not in expected format");
            throw;
        }

        return JsonConvert.SerializeObject(onServerConnected);
    }

    [FunctionName("OnMapChange")]
    [return: ServiceBus("map_change_queue", Connection = "service_bus_connection_string")]
    public static string OnMapChange([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] string input,
        ILogger log)
    {
        OnMapChange onMapChange;
        try
        {
            onMapChange = JsonConvert.DeserializeObject<OnMapChange>(input);
        }
        catch (Exception ex)
        {
            log.LogError($"OnMapChange Raw Input: '{input}'");
            log.LogError(ex, "OnMapChange was not in expected format");
            throw;
        }

        return JsonConvert.SerializeObject(onMapChange);
    }
}