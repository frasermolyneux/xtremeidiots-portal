using System;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models
{
    public class RelatedPlayerDto
    {
        public string GameType { get; set; }
        public string Username { get; set; }
        public Guid PlayerId { get; set; }
        public string IpAddress { get; set; }
    }
}