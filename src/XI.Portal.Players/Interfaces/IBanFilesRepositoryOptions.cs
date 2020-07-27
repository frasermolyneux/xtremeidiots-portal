namespace XI.Portal.Players.Interfaces
{
    public interface IBanFilesRepositoryOptions
    {
        string StorageConnectionString { get; set; }
        string StorageContainerName { get; set; }

        void Validate();
    }
}