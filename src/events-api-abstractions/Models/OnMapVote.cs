namespace XtremeIdiots.Portal.EventsApi.Abstractions.Models;

public class OnMapVote : OnEventBase
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public string MapName { get; set; }
    public string Guid { get; set; }
    public bool Like { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}