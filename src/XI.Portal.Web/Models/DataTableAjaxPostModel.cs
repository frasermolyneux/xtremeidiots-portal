using System.Collections.Generic;
using Newtonsoft.Json;

namespace XI.Portal.Web.Models
{
    public class DataTableAjaxPostModel
    {
        [JsonProperty("draw")] public int Draw { get; set; }
        [JsonProperty("start")] public int Start { get; set; }
        [JsonProperty("length")] public int Length { get; set; }
        [JsonProperty("columns")] public List<DataTableColumn> Columns { get; set; }
        [JsonProperty("search")] public DataTableSearch Search { get; set; }
        [JsonProperty("order")] public List<DataTableOrder> Order { get; set; }
    }
}