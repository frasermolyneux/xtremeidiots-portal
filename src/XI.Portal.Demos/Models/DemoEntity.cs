using System;
using Microsoft.Azure.Cosmos.Table;
using XI.Demos.Constants;

namespace XI.Portal.Demos.Models
{
    public class DemoEntity : TableEntity
    {
        public string UserId { get; set; }
        public GameType Game { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public string Map { get; set; }
        public string Mod { get; set; }
        public string Server { get; set; }
        public long Size { get; set; }
        public Uri BlobUri { get; set; }
    }
}