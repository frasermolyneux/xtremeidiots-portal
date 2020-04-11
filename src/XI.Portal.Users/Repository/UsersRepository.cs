using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos.Table.Queryable;
using XI.Portal.Auth.Data;
using XI.Portal.Auth.Models;
using XI.Portal.Users.Configuration;
using XI.Portal.Users.Models;

namespace XI.Portal.Users.Repository
{
    public class UsersRepository : IUsersRepository
    {
        private readonly ApplicationAuthDbContext _authContext;
        private readonly IUsersRepositoryOptions _options;

        public UsersRepository(IUsersRepositoryOptions options, ApplicationAuthDbContext authContext)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _authContext = authContext ?? throw new ArgumentNullException(nameof(authContext));
        }

        public async Task<List<UserListEntryViewModel>> GetUsers()
        {
            var query = new TableQuery<PortalIdentityUser>().Where(TableQuery.GenerateFilterCondition("Email", QueryComparisons.NotEqual, "")).AsTableQuery();

            var results = new List<UserListEntryViewModel>();

            TableContinuationToken continuationToken = null;
            do
            {
                var queryResult = await _authContext.UserTable.ExecuteQuerySegmentedAsync(query, continuationToken);
                foreach (var entity in queryResult)
                    results.Add(new UserListEntryViewModel
                    {
                        Id = entity.Id,
                        Username = entity.UserName,
                        Email = entity.Email
                    });

                continuationToken = queryResult.ContinuationToken;
            } while (continuationToken != null);

            return results;
        }
    }
}