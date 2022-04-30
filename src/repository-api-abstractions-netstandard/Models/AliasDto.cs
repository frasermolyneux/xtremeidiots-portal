using System;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models
{
    public class AliasDto
    {
        public string Name { get; set; }
        public DateTime Added { get; set; }
        public DateTime LastUsed { get; set; }
    }
}