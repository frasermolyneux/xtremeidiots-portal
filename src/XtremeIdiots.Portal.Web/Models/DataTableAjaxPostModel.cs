using Newtonsoft.Json;

namespace XtremeIdiots.Portal.Web.Models
{

    public class DataTableAjaxPostModel
    {
        [JsonProperty("draw")]
        public int Draw { get; set; }

        [JsonProperty("start")]
        public int Start { get; set; }

        [JsonProperty("length")]
        public int Length { get; set; }

        [JsonProperty("columns")]
        public required List<DataTableColumn> Columns { get; set; }

        [JsonProperty("search")]
        public required DataTableSearch Search { get; set; }

        [JsonProperty("order")]
        public required List<DataTableOrder> Order { get; set; }
    }
}