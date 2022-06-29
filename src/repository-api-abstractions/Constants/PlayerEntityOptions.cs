namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants
{
    [Flags]
    public enum PlayerEntityOptions
    {
        None = 0,
        Aliases = 1,
        IpAddresses = 2,
        AdminActions = 4
    }
}
