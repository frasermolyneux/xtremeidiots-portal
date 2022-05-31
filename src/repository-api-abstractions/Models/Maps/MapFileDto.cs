using Newtonsoft.Json;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Maps
{
    public class MapFileDto
    {
        [JsonProperty]
        public string FileName { get; set; } = string.Empty;

        [JsonProperty]
        public string Url { get; set; } = string.Empty;
    }
}