using System.Collections.Generic;

namespace XtremeIdiots.Portal.CommonLib.Models
{
    public class ChatMessageSearchResponseDto
    {
        public int TotalRecords { get; set; }
        public int FilteredRecords { get; set; }
        public List<ChatMessageSearchEntryDto> Entries { get; set; } = new List<ChatMessageSearchEntryDto>();
    }
}
