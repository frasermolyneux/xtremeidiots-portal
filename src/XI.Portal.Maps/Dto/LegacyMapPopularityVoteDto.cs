using System;

namespace XI.Portal.Maps.Dto
{
    public class LegacyMapPopularityVoteDto
    {
        public Guid ServerId { get; set; }
        public string ServerName { get; set; }
        public Guid PlayerId { get; set; }
        public string PlayerName { get; set; }
        public string ModName { get; set; }
        public int PlayerCount { get; set; }
        public DateTime Updated { get; set; }
        public bool Like { get; set; }
    }
}