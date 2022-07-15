namespace XtremeIdiots.Portal.RepositoryApiClient
{
    public class RepositoryApiClientOptions
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ApiPathPrefix { get; set; } = "repository";

        public bool UseMemoryCacheOnGet { get; set; } = true;
        public int MemoryCacheOnGetExpiration { get; set; } = 30;
    }
}