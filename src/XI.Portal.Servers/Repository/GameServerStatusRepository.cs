using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos.Table.Queryable;
using XI.Portal.Auth.Contract.Extensions;
using XI.Portal.Servers.Configuration;
using XI.Portal.Servers.Interfaces;
using XI.Portal.Servers.Models;
using XI.Servers.Dto;
using XI.Servers.Interfaces;

namespace XI.Portal.Servers.Repository
{
    public class GameServerStatusRepository : IGameServerStatusRepository
    {
        private readonly IGameServerClientFactory _gameServerClientFactory;
        private readonly IGameServersRepository _gameServersRepository;
        private readonly CloudTable _statusTable;

        public GameServerStatusRepository(IGameServerStatusRepositoryOptions options,
            IGameServersRepository gameServersRepository,
            IGameServerClientFactory gameServerClientFactory)
        {
            _gameServersRepository = gameServersRepository ?? throw new ArgumentNullException(nameof(gameServersRepository));
            _gameServerClientFactory = gameServerClientFactory ?? throw new ArgumentNullException(nameof(gameServerClientFactory));
            var storageAccount = CloudStorageAccount.Parse(options.StorageConnectionString);
            var cloudTableClient = storageAccount.CreateCloudTableClient();

            _statusTable = cloudTableClient.GetTableReference(options.StorageTableName);
            _statusTable.CreateIfNotExists();
        }

        public async Task<GameServerStatusDto> GetStatus(Guid serverId, ClaimsPrincipal user, string[] requiredClaims, TimeSpan cacheCutoff)
        {
            var tableOperation = TableOperation.Retrieve<GameServerStatusEntity>("status", serverId.ToString());
            var result = await _statusTable.ExecuteAsync(tableOperation);

            if (result.HttpStatusCode == 404)
            {
                var gameServerDto = await RefreshGameServerStatus(serverId);

                return !UserHasRequiredPermission(user, requiredClaims, gameServerDto) ? null : gameServerDto;
            }

            var storedGameServerStatus = (GameServerStatusEntity) result.Result;

            if (storedGameServerStatus.Timestamp < DateTime.UtcNow + cacheCutoff)
            {
                var gameServerDto = await RefreshGameServerStatus(serverId);

                return !UserHasRequiredPermission(user, requiredClaims, gameServerDto) ? null : gameServerDto;
            }

            var gameServerStatusDto = new GameServerStatusDto
            {
                ServerId = storedGameServerStatus.ServerId,
                GameType = storedGameServerStatus.GameType,
                Hostname = storedGameServerStatus.Hostname,
                QueryPort = storedGameServerStatus.QueryPort,
                ServerName = storedGameServerStatus.ServerName,
                Map = storedGameServerStatus.Map,
                Mod = storedGameServerStatus.Mod,
                PlayerCount = storedGameServerStatus.PlayerCount,
                MaxPlayers = storedGameServerStatus.MaxPlayers,
                Players = storedGameServerStatus.Players
            };

            return !UserHasRequiredPermission(user, requiredClaims, gameServerStatusDto) ? null : gameServerStatusDto;
        }

        public async Task UpdateStatus(Guid id, GameServerStatusDto model)
        {
            var gameServerStatusEntity = new GameServerStatusEntity(id, model);

            var operation = TableOperation.InsertOrMerge(gameServerStatusEntity);
            await _statusTable.ExecuteAsync(operation);
        }

        public async Task<List<GameServerStatusDto>> GetAllStatusModels(ClaimsPrincipal user, string[] requiredClaims, TimeSpan cacheCutoff)
        {
            var query = new TableQuery<GameServerStatusEntity>().AsTableQuery();

            var results = new List<GameServerStatusDto>();

            TableContinuationToken continuationToken = null;
            do
            {
                var queryResult = await _statusTable.ExecuteQuerySegmentedAsync(query, continuationToken);
                foreach (var entity in queryResult)
                {
                    if (entity.Timestamp < DateTime.UtcNow + cacheCutoff)
                    {
                        var refreshedGameServerStatusDto = await RefreshGameServerStatus(entity.ServerId);

                        if (UserHasRequiredPermission(user, requiredClaims, refreshedGameServerStatusDto))
                            results.Add(refreshedGameServerStatusDto);

                        continue;
                    }

                    var gameServerStatusDto = new GameServerStatusDto
                    {
                        ServerId = entity.ServerId,
                        GameType = entity.GameType,
                        ServerName = entity.ServerName,
                        Hostname = entity.Hostname,
                        QueryPort = entity.QueryPort,
                        Map = entity.Map,
                        Mod = entity.Mod,
                        PlayerCount = entity.PlayerCount,
                        MaxPlayers = entity.MaxPlayers,
                        Players = entity.Players
                    };

                    if (UserHasRequiredPermission(user, requiredClaims, gameServerStatusDto))
                        results.Add(gameServerStatusDto);
                }

                continuationToken = queryResult.ContinuationToken;
            } while (continuationToken != null);

            return results;
        }

        private async Task<GameServerStatusDto> RefreshGameServerStatus(Guid serverId)
        {
            var server = await _gameServersRepository.GetGameServer(serverId, null, null);
            var gameServerStatusHelper = _gameServerClientFactory.GetGameServerStatusHelper(server.GameType, server.ServerId, server.Hostname, server.QueryPort, server.RconPassword);

            var gameServerStatus = await gameServerStatusHelper.GetServerStatus();

            await UpdateStatus(serverId, gameServerStatus);
            return gameServerStatus;
        }

        private bool UserHasRequiredPermission(ClaimsPrincipal claimsPrincipal, string[] requiredClaims, GameServerStatusDto gameServerStatusDto)
        {
            if (claimsPrincipal == null && requiredClaims == null) return true;

            var (gameTypes, serverIds) = claimsPrincipal.ClaimedGamesAndServers(requiredClaims);

            if (serverIds.Contains(gameServerStatusDto.ServerId)) return true;

            if (gameTypes.Contains(gameServerStatusDto.GameType))
                return true;

            return false;
        }
    }
}