namespace XI.Servers.Interfaces.Models
{
    internal interface IGameServerStatus<T>
    {
        string ServerName { get; }
        string Map { get; }
        string Mod { get; }
        int PlayerCount { get; }

        IList<T> Players { get; }
    }
}