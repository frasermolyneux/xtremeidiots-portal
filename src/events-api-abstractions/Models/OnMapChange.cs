﻿namespace XtremeIdiots.Portal.EventsApi.Abstractions.Models;

public class OnMapChange : OnEventBase
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public string GameName { get; set; }
    public string MapName { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}