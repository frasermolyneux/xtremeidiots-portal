using Newtonsoft.Json;

namespace XtremeIdiots.Portal.InvisionApiClient.Models
{
    public class Group
    {
        [JsonProperty("id")] public long Id { get; set; }
        [JsonProperty("name")] public string? Name { get; set; }
        [JsonProperty("formattedName")] public string? FormattedName { get; set; }
    }
}