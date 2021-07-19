namespace XI.Portal.Bus.Client
{
    public class PortalServiceBusOptions
    {
        public string ServiceBusConnectionString { get; set; }
        public string MapVotesQueueName { get; set; }
        public string PlayerAuthQueueName { get; set; }
        public string ChatMessageQueueName { get; set; }
    }
}