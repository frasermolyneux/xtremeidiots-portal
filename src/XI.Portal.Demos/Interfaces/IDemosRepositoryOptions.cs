namespace XI.Portal.Demos.Interfaces
{
    public interface IDemosRepositoryOptions
    {
        string StorageConnectionString { get; set; }
        string StorageContainerName { get; set; }

        void Validate();
    }
}