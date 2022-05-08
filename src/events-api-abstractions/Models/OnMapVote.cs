namespace XtremeIdiots.Portal.EventsApi.Abstractions.Models;

public class OnMapVote : OnEventBase
{
    public string? MapName { get; set; }
    public string? Guid { get; set; }
    public bool? Like { get; set; }
}