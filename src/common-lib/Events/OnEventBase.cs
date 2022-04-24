namespace XtremeIdiots.Portal.CommonLib.Events;

public class OnEventBase
{
    public DateTime EventGeneratedUtc { get; set; }
    public string? GameType { get; set; }
    public string? ServerId { get; set; }
}