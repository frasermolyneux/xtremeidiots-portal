using System;
using XI.CommonTypes;

namespace XI.Portal.Demos.Models
{
    public interface IDemoDto
    {
        string RowKey { get; set; }
        string UserId { get; set; }
        GameType Game { get; set; }
        string Name { get; set; }
        DateTime Created { get; set; }
        string Map { get; set; }
        string Mod { get; set; }
        string Server { get; set; }
        long Size { get; set; }
    }
}