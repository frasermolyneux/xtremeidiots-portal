using Newtonsoft.Json;

namespace XI.Portal.Web.Models
{
    public class DataTableSearch
    {
        [JsonProperty("value")] public string Value { get; set; }
        [JsonProperty("regex")] public string Regex { get; set; }
    }
}