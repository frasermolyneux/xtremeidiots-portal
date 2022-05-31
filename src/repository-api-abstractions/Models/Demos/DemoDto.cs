using Newtonsoft.Json;
using System.Text.Json.Serialization;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Demos
{
    public class DemoDto
    {
        [JsonProperty]
        public Guid DemoId { get; set; }

        [JsonProperty]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        public GameType Game { get; set; }

        [JsonProperty]
        public string Name { get; set; } = string.Empty;

        [JsonProperty]
        public string FileName { get; set; } = string.Empty;

        [JsonProperty]
        public DateTime? Date { get; set; }

        [JsonProperty]
        public string Map { get; set; } = string.Empty;

        [JsonProperty]
        public string Mod { get; set; } = string.Empty;

        [JsonProperty]
        public string GameType { get; set; } = string.Empty;

        [JsonProperty]
        public string Server { get; set; } = string.Empty;

        [JsonProperty]
        public long Size { get; set; }

        [JsonProperty]
        public string UserId { get; set; } = string.Empty;

        [JsonProperty]
        public string UploadedBy { get; set; } = string.Empty;

        [JsonProperty]
        public string DemoFileUri { get; set; } = string.Empty;
    }
}