using System.Collections.Generic;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models
{
    public class ChatMessageSearchResponseDto
    {
        public int TotalRecords { get; set; }
        public int FilteredRecords { get; set; }
        public List<ChatMessageSearchEntryDto> Entries { get; set; } = new List<ChatMessageSearchEntryDto>();
    }
}
