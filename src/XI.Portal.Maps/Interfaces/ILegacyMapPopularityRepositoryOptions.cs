namespace XI.Portal.Maps.Interfaces
{
    public interface ILegacyMapPopularityRepositoryOptions
    {
        string StorageConnectionString { get; set; }
        string StorageTableName { get; set; }

        void Validate();
    }
}