namespace XtremeIdiots.Portal.Web.Models
{
    public class MapTimelineDataPoint
    {
        public MapTimelineDataPoint(string mapName, DateTime start, DateTime end)
        {
            MapName = mapName;
            Start = start;
            End = end;
        }

        public string MapName { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}