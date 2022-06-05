namespace XtremeIdiots.Portal.ServersApiClient
{
    public class ServersApiClientOptions
    {
        public string ApimBaseUrl { get; set; } = string.Empty;
        public string ApimSubscriptionKey { get; set; } = string.Empty;
        public string ApiPathPrefix { get; set; } = "servers";
    }
}