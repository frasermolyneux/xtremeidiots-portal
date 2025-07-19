using Newtonsoft.Json;

namespace XtremeIdiots.Portal.Web.Models
{
    /// <summary>
    /// Represents ordering parameters from DataTable AJAX requests
    /// </summary>
    public class DataTableOrder
    {
        [JsonProperty("column")]
        public int Column { get; set; }

        [JsonProperty("dir")]
        public required string Dir { get; set; }
    }
}