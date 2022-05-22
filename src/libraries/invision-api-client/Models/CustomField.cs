using Newtonsoft.Json;

namespace XtremeIdiots.Portal.InvisionApiClient.Models
{
    public class CustomField
    {
        [JsonProperty("name")] public string? Name { get; set; }

        [JsonProperty("fields")] public Dictionary<string, Field> Fields { get; set; } = new Dictionary<string, Field>();
    }
}