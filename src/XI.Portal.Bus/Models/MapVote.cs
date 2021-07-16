using XI.CommonTypes;

namespace XI.Portal.Bus.Models
{
    public class MapVote
    {
        public GameType GameType { get; set; }
        public string MapName { get; set; }
        public string Guid { get; set; }
        public bool Like { get; set; }
    }
}