namespace XtremeIdiots.Portal.CommonLib.Events;

public class OnServerConnected
{
    public OnServerConnected(string id, string gameType)
    {
        Id = id;
        GameType = gameType;
    }

    public string Id { get; set; }
    public string GameType { get; set; }
}