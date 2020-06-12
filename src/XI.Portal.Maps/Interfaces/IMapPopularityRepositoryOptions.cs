namespace XI.Portal.Maps.Interfaces
{
    public interface IMapPopularityRepositoryOptions
    {
        string StorageConnectionString { get; set; }
        string StorageTableName { get; set; }

        void Validate();
    }
}