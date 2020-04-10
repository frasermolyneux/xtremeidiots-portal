using System;
using XI.Demos.Constants;

namespace XI.Portal.Demos.Models
{
    public class DemoDto : IDemoDto
    {
        public string RowKey { get; set; }
        public string UserId { get; set; }
        public GameType Game { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public string Map { get; set; }
        public string Mod { get; set; }
        public string Server { get; set; }
        public long Size { get; set; }
    }
}