using System;

namespace XI.Portal.Users.Configuration
{
    internal class UsersRepositoryOptions : IUsersRepositoryOptions
    {
        public string StorageConnectionString { get; set; }
        public string StorageTableName { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(StorageConnectionString))
                throw new NullReferenceException(nameof(StorageConnectionString));

            if (string.IsNullOrWhiteSpace(StorageTableName))
                throw new NullReferenceException(nameof(StorageTableName));
        }
    }
}