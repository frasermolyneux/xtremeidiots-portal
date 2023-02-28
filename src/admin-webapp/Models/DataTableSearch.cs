using Newtonsoft.Json;

namespace XtremeIdiots.Portal.AdminWebApp.Models
{
    public class DataTableSearch
    {
        [JsonProperty("value")] public string Value { get; set; }
        [JsonProperty("regex")] public string Regex { get; set; }
    }
}