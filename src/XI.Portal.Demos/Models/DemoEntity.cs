using System;
using Microsoft.Azure.Cosmos.Table;
using XI.CommonTypes;

namespace XI.Portal.Demos.Models
{
    internal class DemoEntity : TableEntity, IDemoDto
    {
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