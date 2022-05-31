using Newtonsoft.Json;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.ChatMessages
{
    public class ChatMessageSearchResponseDto
    {
        [JsonProperty]
        public int TotalRecords { get; set; }

        [JsonProperty]
        public int FilteredRecords { get; set; }

        [JsonProperty]
        public List<ChatMessageSearchEntryDto> Entries { get; set; } = new List<ChatMessageSearchEntryDto>();
    }
}
