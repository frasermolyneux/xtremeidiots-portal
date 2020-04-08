using Newtonsoft.Json;

namespace XI.Portal.Web.Models
{
    public class DataTableOrder
    {
        [JsonProperty("column")] public int Column { get; set; }
        [JsonProperty("dir")] public string Dir { get; set; }
    }
}