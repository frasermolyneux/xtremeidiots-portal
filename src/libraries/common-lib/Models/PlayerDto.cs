namespace XtremeIdiots.Portal.CommonLib.Models
{
    public class PlayerDto
    {
        public Guid Id { get; set; }
        public string GameType { get; set; }
        public string Username { get; set; }
        public string Guid { get; set; }
        public DateTime FirstSeen { get; set; }
        public DateTime LastSeen { get; set; }
        public string IpAddress { get; set; }
    }
}