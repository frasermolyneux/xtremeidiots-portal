using Newtonsoft.Json;

namespace XtremeIdiots.Portal.Web.Models
{
    /// <summary>
    /// Represents search parameters from DataTable AJAX requests
    /// </summary>
    public class DataTableSearch
    {
        [JsonProperty("value")]
        public required string Value { get; set; }

        [JsonProperty("regex")]
        public required string Regex { get; set; }
    }
}