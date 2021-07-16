namespace XI.Portal.Bus.Client
{
    public class PortalServiceBusOptions
    {
        public string ServiceBusConnectionString { get; set; }
        public string MapVotesQueueName { get; set; }
    }
}