using System;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models
{
    public class DemoAuthDto
    {
        public string UserId { get; set; } = string.Empty;
        public string AuthKey { get; set; } = string.Empty;
        public DateTime Created { get; set; }
        public DateTime LastActivity { get; set; }
    }
}
