namespace XI.Portal.Players.Interfaces
{
    public interface IPlayersCacheRepositoryOptions
    {
        string StorageConnectionString { get; set; }
        string StorageTableName { get; set; }

        void Validate();
    }
}