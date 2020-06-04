using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FM.GeoLocation.Contract.Interfaces;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos.Table.Queryable;
using XI.Portal.Players.Dto;
using XI.Portal.Players.Interfaces;
using XI.Portal.Servers.Dto;
using XI.Portal.Servers.Interfaces;
using XI.Portal.Servers.Models;
using XI.Servers.Interfaces;

namespace XI.Portal.Servers.Repository
{
    public class GameServerStatusRepository : IGameServerStatusRepository
    {
        private readonly IGameServerClientFactory _gameServerClientFactory;
        private readonly IGameServersRepository _gameServersRepository;
        private readonly IGeoLocationClient _geoLocationClient;
        private readonly IPlayerLocationsRepository _playersLocationsRepository;
        private readonly CloudTable _statusTable;

        public GameServerStatusRepository(IGameServerStatusRepositoryOptions options,
            IGameServersRepository gameServersRepository,
            IGameServerClientFactory gameServerClientFactory,
            IGeoLocationClient geoLocationClient,
            IPlayerLocationsRepository playersLocationsRepository)
        {
            _gameServersRepository = gameServersRepository ?? throw new ArgumentNullException(nameof(gameServersRepository));
            _gameServerClientFactory = gameServerClientFactory ?? throw new ArgumentNullException(nameof(gameServerClientFactory));
            _geoLocationClient = geoLocationClient ?? throw new ArgumentNullException(nameof(geoLocationClient));
            _playersLocationsRepository = playersLocationsRepository ?? throw new ArgumentNullException(nameof(playersLocationsRepository));
            var storageAccount = CloudStorageAccount.Parse(options.StorageConnectionString);
            var cloudTableClient = storageAccount.CreateCloudTableClient();

            _statusTable = cloudTableClient.GetTableReference(options.StorageTableName);
            _statusTable.CreateIfNotExists();
        }

        public async Task<PortalGameServerStatusDto> GetStatus(Guid serverId, TimeSpan cacheCutoff)
        {
            var tableOperation = TableOperation.Retrieve<PortalGameServerStatusEntity>("status", serverId.ToString());
            var result = await _statusTable.ExecuteAsync(tableOperation);

            if (result.HttpStatusCode == 404)
            {
                return await RefreshGameServerStatus(serverId);
            }

            var storedGameServerStatus = (PortalGameServerStatusEntity) result.Result;

            if (cacheCutoff != TimeSpan.Zero && storedGameServerStatus.Timestamp < DateTime.UtcNow + cacheCutoff)
            {
                return await RefreshGameServerStatus(serverId);
            }

            var gameServerStatusDto = new PortalGameServerStatusDto
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
                Players = storedGameServerStatus.Players,
                Timestamp = storedGameServerStatus.Timestamp
            };

            return gameServerStatusDto;
        }

        public async Task UpdateStatus(Guid id, PortalGameServerStatusDto model)
        {
            var gameServerStatusEntity = new PortalGameServerStatusEntity(id, model);

            var operation = TableOperation.InsertOrMerge(gameServerStatusEntity);
            await _statusTable.ExecuteAsync(operation);
        }

        public async Task<List<PortalGameServerStatusDto>> GetAllStatusModels(GameServerStatusFilterModel filterModel, TimeSpan cacheCutoff)
        {
            var query = new TableQuery<PortalGameServerStatusEntity>().AsTableQuery();

            var results = new List<PortalGameServerStatusDto>();

            TableContinuationToken continuationToken = null;
            do
            {
                var queryResult = await _statusTable.ExecuteQuerySegmentedAsync(query, continuationToken);
                foreach (var entity in queryResult)
                {
                    if (cacheCutoff != TimeSpan.Zero && entity.Timestamp < DateTime.UtcNow + cacheCutoff)
                    {
                        var refreshedGameServerStatusDto = await RefreshGameServerStatus(entity.ServerId);
                        results.Add(refreshedGameServerStatusDto);
                        continue;
                    }

                    var gameServerStatusDto = new PortalGameServerStatusDto
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
                        Players = entity.Players,
                        Timestamp = entity.Timestamp
                    };

                    results.Add(gameServerStatusDto);
                }

                continuationToken = queryResult.ContinuationToken;
            } while (continuationToken != null);

            var toReturn = results.Where(server => server != null).AsQueryable();

            if (filterModel.GameTypes != null && filterModel.GameTypes.Any())
            {
                toReturn = toReturn.Where(server => filterModel.GameTypes.Contains(server.GameType)).AsQueryable();
            }

            if (filterModel.ServerIds != null && filterModel.ServerIds.Any())
            {
                toReturn = toReturn.Where(server => filterModel.ServerIds.Contains(server.ServerId)).AsQueryable();
            }

            return toReturn.ToList();
        }

        private async Task<PortalGameServerStatusDto> RefreshGameServerStatus(Guid serverId)
        {
            try
            {
                var server = await _gameServersRepository.GetGameServer(serverId);
                var gameServerStatusHelper = _gameServerClientFactory.GetGameServerStatusHelper(server.GameType, server.ServerId, server.Hostname, server.QueryPort, server.RconPassword);

                var gameServerStatus = await gameServerStatusHelper.GetServerStatus();

                var model = new PortalGameServerStatusDto
                {
                    ServerId = serverId,
                    GameType = server.GameType,
                    Hostname = server.Hostname,
                    QueryPort = server.QueryPort,
                    MaxPlayers = gameServerStatus.MaxPlayers,
                    ServerName = gameServerStatus.ServerName,
                    Map = gameServerStatus.Map,
                    Mod = gameServerStatus.Mod,
                    PlayerCount = gameServerStatus.PlayerCount
                };

                var players = new List<PortalGameServerPlayerDto>();

                foreach (var player in gameServerStatus.Players)
                {
                    var playerDto = new PortalGameServerPlayerDto
                    {
                        Num = player.Num,
                        Guid = player.Guid,
                        Name = player.Name,
                        IpAddress = player.IpAddress,
                        Score = player.Score,
                        Rate = player.Rate
                    };

                    if (!string.IsNullOrWhiteSpace(player.IpAddress))
                    {
                        var geoLocationResponse = await _geoLocationClient.LookupAddress(playerDto.IpAddress);

                        if (geoLocationResponse.Success)
                        {
                            playerDto.GeoLocation = geoLocationResponse.GeoLocationDto;

                            await _playersLocationsRepository.UpdateEntry(new PlayerLocationDto
                            {
                                GameType = server.GameType,
                                ServerId = server.ServerId,
                                ServerName = gameServerStatus.ServerName,
                                Guid = player.Guid,
                                PlayerName = player.Name,
                                GeoLocation = geoLocationResponse.GeoLocationDto
                            });
                        }
                    }

                    players.Add(playerDto);
                }

                model.Players = players;

                await UpdateStatus(serverId, model);
                return model;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}