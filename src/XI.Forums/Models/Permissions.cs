using Newtonsoft.Json;

namespace XI.Forums.Models
{
    public class Permissions
    {
        [JsonProperty("perm_id")] public long PermId { get; set; }
        [JsonProperty("perm_view")] public string PermView { get; set; }
        [JsonProperty("perm_2")] public string Perm2 { get; set; }
        [JsonProperty("perm_3")] public string Perm3 { get; set; }
        [JsonProperty("perm_4")] public string Perm4 { get; set; }
        [JsonProperty("perm_5")] public string Perm5 { get; set; }
        [JsonProperty("perm_6")] public string Perm6 { get; set; }
        [JsonProperty("perm_7")] public string Perm7 { get; set; }
    }
}