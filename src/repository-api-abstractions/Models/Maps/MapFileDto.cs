using Newtonsoft.Json;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Maps
{
    public class MapFileDto
    {
        public MapFileDto(string fileName, string url)
        {
            FileName = fileName;
            Url = url;
        }

        [JsonProperty]
        public string FileName { get; set; }

        [JsonProperty]
        public string Url { get; set; }
    }
}