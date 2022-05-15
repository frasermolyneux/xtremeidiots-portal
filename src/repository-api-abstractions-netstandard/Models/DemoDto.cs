using System;
using System.Text.Json.Serialization;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models
{
    public class DemoDto
    {
        public Guid DemoId { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GameType Game { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Map { get; set; } = string.Empty;
        public string Mod { get; set; } = string.Empty;
        public string GameType { get; set; } = string.Empty;
        public string Server { get; set; } = string.Empty;
        public long Size { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UploadedBy { get; set; } = string.Empty;
        public string DemoFileUri { get; set; } = string.Empty;
    }
}