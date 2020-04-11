using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos.Table.Queryable;
using XI.Portal.Auth.Contract.Models;
using XI.Portal.Users.Configuration;
using XI.Portal.Users.Data;
using XI.Portal.Users.Models;

namespace XI.Portal.Users.Repository
{
    public class UsersRepository : IUsersRepository
    {
        private readonly CloudTable _additionalClaimsTable;
        private readonly ApplicationAuthDbContext _authContext;
        private readonly IUsersRepositoryOptions _options;

        public UsersRepository(IUsersRepositoryOptions options, ApplicationAuthDbContext authContext)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _authContext = authContext ?? throw new ArgumentNullException(nameof(authContext));

            var storageAccount = CloudStorageAccount.Parse(options.StorageConnectionString);
            var cloudTableClient = storageAccount.CreateCloudTableClient();

            _additionalClaimsTable = cloudTableClient.GetTableReference(options.StorageTableName);
            _additionalClaimsTable.CreateIfNotExists();
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
                        Email = entity.Email,
                        PortalClaims = await GetUserClaims(entity.Id)
                    });

                continuationToken = queryResult.ContinuationToken;
            } while (continuationToken != null);

            return results;
        }

        public async Task<List<IPortalClaimDto>> GetUserClaims(string userId)
        {
            var query = new TableQuery<PortalClaimEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userId)).AsTableQuery();

            var results = new List<IPortalClaimDto>();

            TableContinuationToken continuationToken = null;
            do
            {
                var queryResult = await _additionalClaimsTable.ExecuteQuerySegmentedAsync(query, continuationToken);
                foreach (var entity in queryResult)
                    results.Add(new PortalClaimDto
                    {
                        RowKey = entity.RowKey,
                        ClaimType = entity.ClaimType,
                        ClaimValue = entity.ClaimValue
                    });

                continuationToken = queryResult.ContinuationToken;
            } while (continuationToken != null);

            return results;
        }

        public async Task UpdatePortalClaim(string userId, PortalClaimDto portalClaimDto)
        {
            var portalClaimEntity = new PortalClaimEntity
            {
                RowKey = portalClaimDto.RowKey,
                ClaimType = portalClaimDto.ClaimType,
                ClaimValue = portalClaimDto.ClaimValue
            };

            if (string.IsNullOrWhiteSpace(portalClaimEntity.RowKey)) portalClaimEntity.RowKey = Guid.NewGuid().ToString();

            portalClaimEntity.PartitionKey = userId;

            var operation = TableOperation.InsertOrMerge(portalClaimEntity);
            await _additionalClaimsTable.ExecuteAsync(operation);
        }

        public async Task<UserListEntryViewModel> GetUser(string userId)
        {
            var query = new TableQuery<PortalIdentityUser>().Where(TableQuery.GenerateFilterCondition("Id", QueryComparisons.Equal, userId)).AsTableQuery();

            var user = _authContext.UserTable.ExecuteQuery(query).First();
            var portalClaims = await GetUserClaims(userId);

            return new UserListEntryViewModel
            {
                Id = userId,
                Username = user.UserName,
                Email = user.Email,
                PortalClaims = portalClaims
            };
        }

        public async Task RemoveUserClaim(string userId, string claimId)
        {
            var tableOperation = TableOperation.Retrieve<PortalClaimEntity>(userId, claimId);
            var result = await _additionalClaimsTable.ExecuteAsync(tableOperation);

            var operation = TableOperation.Delete((PortalClaimEntity) result.Result);
            await _additionalClaimsTable.ExecuteAsync(operation);
        }
    }
}