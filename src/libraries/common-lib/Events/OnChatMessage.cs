﻿namespace XtremeIdiots.Portal.CommonLib.Events;

public class OnChatMessage : OnEventBase
{
    public string? Username { get; set; }
    public string? Guid { get; set; }
    public string? Message { get; set; }
    public string? Type { get; set; }
}