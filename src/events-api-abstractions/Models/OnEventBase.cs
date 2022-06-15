namespace XtremeIdiots.Portal.EventsApi.Abstractions.Models;

public class OnEventBase
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public DateTime EventGeneratedUtc { get; set; }
    public string GameType { get; set; }
    public Guid ServerId { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}