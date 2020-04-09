namespace XI.Portal.Maps.Configuration
{
    public interface IMapImageRepositoryOptions
    {
        string StorageConnectionString { get; set; }
        string StorageContainerName { get; set; }

        void Validate();
    }
}