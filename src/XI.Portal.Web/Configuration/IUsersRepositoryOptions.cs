namespace XI.Portal.Web.Configuration
{
    public interface IUsersRepositoryOptions
    {
        string StorageConnectionString { get; set; }
        string StorageTableName { get; set; }

        void Validate();
    }
}