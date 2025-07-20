namespace XtremeIdiots.Portal.Web.Models;

public class MapTimelineDataPoint(string mapName, DateTime start, DateTime end)
{
    public string MapName { get; set; } = mapName;
    public DateTime Start { get; set; } = start;
    public DateTime End { get; set; } = end;
}