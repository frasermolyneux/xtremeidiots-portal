namespace XI.Portal.Maps.Interfaces
{
    public interface IMapImageRepositoryOptions
    {
        string StorageConnectionString { get; set; }
        string StorageContainerName { get; set; }

        void Validate();
    }
}