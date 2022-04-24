namespace XtremeIdiots.Portal.CommonLib.Events;

public class OnPlayerConnected : OnEventBase
{
    public string? Username { get; set; }
    public string? Guid { get; set; }
    public string? IpAddress { get; set; }
}