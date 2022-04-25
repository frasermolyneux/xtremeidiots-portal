namespace XtremeIdiots.Portal.CommonLib.Models
{
    public class ChatMessage
    {
        public Guid Id { get; set; }
        public string GameServerId { get; set; }
        public Guid PlayerId { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public DateTime Timestamp { get; set; }
    }
}