namespace XtremeIdiots.Portal.ServersWebApi.Interfaces
{
    public interface IQueryResponse
    {
        string ServerName { get; }
        string Map { get; }
        string Mod { get; }
        int MaxPlayers { get; }
        int PlayerCount { get; }

        IDictionary<string, string> ServerParams { get; }
        IList<IQueryPlayer> Players { get; }
    }
}