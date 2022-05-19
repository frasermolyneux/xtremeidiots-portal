namespace XtremeIdiots.Portal.RepositoryApiClient
{
    public class RepositoryApiClientOptions
    {
        public string ApimBaseUrl { get; set; }
        public string ApimSubscriptionKey { get; set; }
        public bool UseMemoryCacheOnGet { get; set; } = true;
        public int MemoryCacheOnGetExpiration { get; set; } = 30;
    }
}