namespace XI.Servers.Interfaces.Models
{
    internal interface IGameServerPlayer
    {
        string Num { get; }
        string Guid { get; }
        string Name { get; }
        string IpAddress { get; }
        int Score { get; }
        string Rate { get; }

        string NormalizedName { get; }
    }
}