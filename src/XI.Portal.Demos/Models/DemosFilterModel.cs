using XI.CommonTypes;

namespace XI.Portal.Demos.Models
{
    public class DemosFilterModel
    {
        public enum OrderBy
        {
            GameTypeAsc,
            GameTypeDesc,
            NameAsc,
            NameDesc,
            DateAsc,
            DateDesc,
            UploadedByAsc,
            UploadedByDesc
        }

        public GameType GameType { get; set; }
        public OrderBy Order { get; set; } = OrderBy.UploadedByDesc;
        public string FilterString { get; set; }
        public int SkipEntries { get; set; } = 0;
        public int TakeEntries { get; set; } = 0;
    }
}