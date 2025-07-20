namespace XtremeIdiots.Portal.Web.Models;

/// <summary>
/// Represents a data point for map timeline visualization showing when a specific map was active
/// </summary>
/// <param name="mapName">The name of the map</param>
/// <param name="start">The start time when the map became active</param>
/// <param name="end">The end time when the map was no longer active</param>
public class MapTimelineDataPoint(string mapName, DateTime start, DateTime end)
{
    /// <summary>
    /// Gets or sets the name of the map
    /// </summary>
    public string MapName { get; set; } = mapName;

    /// <summary>
    /// Gets or sets the start time when the map became active
    /// </summary>
    public DateTime Start { get; set; } = start;

    /// <summary>
    /// Gets or sets the end time when the map was no longer active
    /// </summary>
    public DateTime End { get; set; } = end;
}