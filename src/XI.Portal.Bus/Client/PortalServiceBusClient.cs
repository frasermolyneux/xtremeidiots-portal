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
        public ServiceBusSender PlayerAuthServiceBus { get; set; }
        public ServiceBusSender ChatMesageServiceBus { get; set; }

        public PortalServiceBusClient(IOptions<PortalServiceBusOptions> options)
        {
            BusClient = new ServiceBusClient(options.Value.ServiceBusConnectionString);
            MapVotesServiceBus = BusClient.CreateSender(options.Value.MapVotesQueueName);
            PlayerAuthServiceBus = BusClient.CreateSender(options.Value.PlayerAuthQueueName);
            ChatMesageServiceBus = BusClient.CreateSender(options.Value.ChatMessageQueueName);
        }

        public async Task PostMapVote(MapVote model)
        {
            var messageAsString = JsonConvert.SerializeObject(model);
            await MapVotesServiceBus.SendMessageAsync(new ServiceBusMessage(messageAsString));
        }

        public async Task PostPlayerAuth(PlayerAuth playerAuth)
        {
            var messageAsString = JsonConvert.SerializeObject(playerAuth);
            await PlayerAuthServiceBus.SendMessageAsync(new ServiceBusMessage(messageAsString));
        }

        public async Task PostChatMessageEvent(ChatMessage chatMessage)
        {
            var messageAsString = JsonConvert.SerializeObject(chatMessage);
            await ChatMesageServiceBus.SendMessageAsync(new ServiceBusMessage(messageAsString));
        }
    }
}