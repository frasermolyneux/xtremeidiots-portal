using Newtonsoft.Json;

namespace XtremeIdiots.Portal.Web.Models
{

    public class DataTableColumn
    {
        [JsonProperty("data")]
        public required string Data { get; set; }

        [JsonProperty("name")]
        public required string Name { get; set; }

        [JsonProperty("searchable")]
        public required string Searchable { get; set; }

        [JsonProperty("orderable")]
        public required string Orderable { get; set; }

        [JsonProperty("search")]
        public required DataTableSearch Search { get; set; }
    }
}