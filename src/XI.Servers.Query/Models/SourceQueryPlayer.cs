using System;

namespace XI.Servers.Query.Models
{
    internal class SourceQueryPlayer : IQueryPlayer
    {
        public TimeSpan Time { get; set; }
        public string Name { get; set; }
        public int Score { get; set; }

        public string NormalizedName => Name.ToUpper().Trim();
    }
}