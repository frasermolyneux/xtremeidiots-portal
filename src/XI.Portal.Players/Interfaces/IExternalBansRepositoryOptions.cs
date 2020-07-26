namespace XI.Portal.Players.Interfaces
{
    public interface IExternalBansRepositoryOptions
    {
        string StorageConnectionString { get; set; }
        string StorageContainerName { get; set; }

        void Validate();
    }
}