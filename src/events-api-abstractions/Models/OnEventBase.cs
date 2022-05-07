namespace XtremeIdiots.Portal.EventsApi.Abstractions.Models;

public class OnEventBase
{
    public DateTime EventGeneratedUtc { get; set; }
    public string? GameType { get; set; }
    public Guid? ServerId { get; set; }
}