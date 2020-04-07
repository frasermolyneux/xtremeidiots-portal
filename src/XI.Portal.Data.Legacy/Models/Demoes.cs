using System;

namespace XI.Portal.Data.Legacy.Models
{
    public class Demoes
    {
        public Guid DemoId { get; set; }
        public int Game { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        public DateTime Date { get; set; }
        public string Map { get; set; }
        public string Mod { get; set; }
        public string GameType { get; set; }
        public string Server { get; set; }
        public long Size { get; set; }
        public string UserId { get; set; }

        public virtual AspNetUsers User { get; set; }
    }
}