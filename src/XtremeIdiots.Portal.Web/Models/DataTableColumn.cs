using Newtonsoft.Json;

namespace XtremeIdiots.Portal.Web.Models
{
    public class DataTableColumn
    {
        [JsonProperty("data")] public string Data { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("searchable")] public string Searchable { get; set; }
        [JsonProperty("orderable")] public string Orderable { get; set; }
        [JsonProperty("search")] public DataTableSearch Search { get; set; }
    }
}