using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using XI.Portal.Bus.Models;

namespace XI.Portal.Bus.Client
{
    public class PortalServiceBusClient : IPortalServiceBusClient
    {
        public ServiceBusClient BusClient { get; set; }
        public ServiceBusSender MapVotesServiceBus { get; set; }

        public PortalServiceBusClient(IOptions<PortalServiceBusOptions> options)
        {
            BusClient = new ServiceBusClient(options.Value.ServiceBusConnectionString);
            MapVotesServiceBus = BusClient.CreateSender(options.Value.MapVotesQueueName);
        }

        public async Task PostMapVote(MapVote model)
        {
            var messageAsString = JsonConvert.SerializeObject(model);
            await MapVotesServiceBus.SendMessageAsync(new ServiceBusMessage(messageAsString));
        }
    }
}