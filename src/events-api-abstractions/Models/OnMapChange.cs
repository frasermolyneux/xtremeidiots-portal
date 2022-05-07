namespace XtremeIdiots.Portal.EventsApi.Abstractions.Models;

public class OnMapChange : OnEventBase
{
    public string? GameName { get; set; }
    public string? MapName { get; set; }
}