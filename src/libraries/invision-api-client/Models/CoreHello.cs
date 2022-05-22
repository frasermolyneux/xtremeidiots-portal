using Newtonsoft.Json;

namespace XtremeIdiots.Portal.InvisionApiClient.Models
{
    public class CoreHello
    {
        [JsonProperty("communityName")] public string? CommunityName { get; set; }
        [JsonProperty("communityUrl")] public string? CommunityUrl { get; set; }
        [JsonProperty("ipsVersion")] public string? IpsVersion { get; set; }
    }
}