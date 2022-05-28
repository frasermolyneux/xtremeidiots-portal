namespace XtremeIdiots.Portal.SyncFunc.Interfaces
{
    public interface IBanFilesRepositoryOptions
    {
        string ConnectionString { get; set; }
        string ContainerName { get; set; }
    }
}