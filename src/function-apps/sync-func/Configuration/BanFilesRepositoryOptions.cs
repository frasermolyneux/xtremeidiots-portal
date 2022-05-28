using XtremeIdiots.Portal.SyncFunc.Interfaces;

namespace XtremeIdiots.Portal.SyncFunc.Configuration
{
    internal class BanFilesRepositoryOptions : IBanFilesRepositoryOptions
    {
        public string ConnectionString { get; set; }
        public string ContainerName { get; set; } = "ban-files";
    }
}