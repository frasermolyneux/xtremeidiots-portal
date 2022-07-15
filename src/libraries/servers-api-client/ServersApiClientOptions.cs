namespace XtremeIdiots.Portal.ServersApiClient
{
    public class ServersApiClientOptions
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ApiPathPrefix { get; set; } = "servers";
    }
}