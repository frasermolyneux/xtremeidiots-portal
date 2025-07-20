using Newtonsoft.Json;

namespace XtremeIdiots.Portal.Web.Models;

public class DataTableSearch
{
    [JsonProperty("value")]
    public required string Value { get; set; }

    [JsonProperty("regex")]
    public required string Regex { get; set; }
}