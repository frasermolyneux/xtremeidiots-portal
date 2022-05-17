namespace XI.Portal.Players.Models
{
    public class PlayerListEntryViewModel
    {
        public string GameType { get; internal set; }
        public Guid PlayerId { get; internal set; }
        public string Username { get; internal set; }
        public string Guid { get; internal set; }
        public string IpAddress { get; internal set; }
        public string FirstSeen { get; internal set; }
        public string LastSeen { get; internal set; }
    }
}