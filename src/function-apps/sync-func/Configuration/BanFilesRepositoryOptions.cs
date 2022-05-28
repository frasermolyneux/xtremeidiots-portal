namespace XtremeIdiots.Portal.SyncFunc.Configuration
{
    public class BanFilesRepositoryOptions
    {
        public string? ConnectionString { get; set; }
        public string? ContainerName { get; set; } = "ban-files";
    }
}