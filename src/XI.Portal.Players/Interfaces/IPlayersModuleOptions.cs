namespace XI.Portal.Players.Interfaces
{
    public interface IPlayersModuleOptions
    {
        Action<IBanFilesRepositoryOptions> BanFilesRepositoryOptions { get; set; }
    }
}