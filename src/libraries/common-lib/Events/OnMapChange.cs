namespace XtremeIdiots.Portal.CommonLib.Events;

public class OnMapChange : OnEventBase
{
    public string? GameName { get; set; }
    public string? MapName { get; set; }
}