using XI.CommonTypes;

namespace XI.Portal.Maps.Models
{
    public interface IMapDto
    {
        string RowKey { get; set; }

        GameType GameType { get; set; }
    }
}