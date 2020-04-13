namespace XI.Servers.Rcon.Models
{
    internal class SourceRconPlayer : IRconPlayer
    {
        public string Num { get; set; }
        public string Ping { get; set; }
        public string Guid { get; set; }
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public string Rate { get; set; }

        public string NormalizedName => Name.ToUpper().Trim();
    }
}