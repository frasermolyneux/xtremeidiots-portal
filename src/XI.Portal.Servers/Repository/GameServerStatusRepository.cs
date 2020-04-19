using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using XI.Portal.Auth.Contract.Extensions;
using XI.Portal.Servers.Configuration;
using XI.Portal.Servers.Interfaces;
using XI.Portal.Servers.Models;
using XI.Servers.Dto;
using XI.Servers.Factories;
using XI.Servers.Interfaces;

namespace XI.Portal.Servers.Repository
{
    public class GameServerStatusRepository : IGameServerStatusRepository
    {
        private readonly IGameServersRepository _gameServersRepository;
        private readonly IGameServerClientFactory _gameServerClientFactory;
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

        public async Task<GameServerStatusDto> GetStatus(Guid serverId, ClaimsPrincipal user, IEnumerable<string> requiredClaims, TimeSpan cacheCutoff)
        {
            var tableOperation = TableOperation.Retrieve<GameServerStatusEntity>("status", serverId.ToString());
            var result = await _statusTable.ExecuteAsync(tableOperation);

            if (result.HttpStatusCode == 404)
                return await RefreshGameServerStatus(serverId);

            var storedGameServerStatus = (GameServerStatusEntity) result.Result;

            if (storedGameServerStatus.Timestamp < DateTime.UtcNow + cacheCutoff)
                return await RefreshGameServerStatus(serverId);

            return new GameServerStatusDto
            {
                ServerId = storedGameServerStatus.ServerId,
                GameType = storedGameServerStatus.GameType,
                ServerName = storedGameServerStatus.ServerName,
                Map = storedGameServerStatus.Map,
                Mod = storedGameServerStatus.Mod,
                PlayerCount = storedGameServerStatus.PlayerCount,
                Players = storedGameServerStatus.Players
            };
        }

        public async Task UpdateStatus(Guid id, GameServerStatusDto model)
        {
            var gameServerStatusEntity = new GameServerStatusEntity(id, model);

            var operation = TableOperation.InsertOrMerge(gameServerStatusEntity);
            await _statusTable.ExecuteAsync(operation);
        }

        private async Task<GameServerStatusDto> RefreshGameServerStatus(Guid serverId)
        {
            var server = await _gameServersRepository.GetGameServer(serverId, null, null);
            var gameServerStatusHelper = _gameServerClientFactory.GetGameServerStatusHelper(server.GameType, server.ServerId, server.Hostname, server.QueryPort, server.RconPassword);

            var gameServerStatus = await gameServerStatusHelper.GetServerStatus();

            await UpdateStatus(serverId, gameServerStatus);
            return gameServerStatus;
        }

        private bool UserHasRequiredPermission(ClaimsPrincipal claimsPrincipal, IEnumerable<string> requiredClaims, GameServerStatusDto gameServerStatusDto)
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