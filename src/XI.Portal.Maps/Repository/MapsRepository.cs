using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.EntityFrameworkCore;
using XI.CommonTypes;
using XI.Portal.Data.Legacy;
using XI.Portal.Maps.Configuration;
using XI.Portal.Maps.Extensions;
using XI.Portal.Maps.Models;

namespace XI.Portal.Maps.Repository
{
    public class MapsRepository : IMapsRepository
    {
        private readonly LegacyPortalContext _legacyContext;
        private readonly CloudTable _mapsTable;
        private readonly IMapsRepositoryOptions _options;

        public MapsRepository(IMapsRepositoryOptions options, LegacyPortalContext legacyContext)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));

            var storageAccount = CloudStorageAccount.Parse(options.StorageConnectionString);
            var cloudTableClient = storageAccount.CreateCloudTableClient();

            _mapsTable = cloudTableClient.GetTableReference(options.StorageTableName);
            _mapsTable.CreateIfNotExists();
        }

        public async Task<IMapDto> GetGameMap(GameType gameType, string mapName)
        {
            var tableOperation = TableOperation.Retrieve<MapEntity>(gameType.ToString(), mapName);
            var result = await _mapsTable.ExecuteAsync(tableOperation);

            if (result.HttpStatusCode == 404)
                return null;

            var mapDto = (IMapDto)result.Result;
            return mapDto;
        }

        public async Task UpdateMap(IMapDto mapDto)
        {
            var mapEntity = new MapEntity {RowKey = mapDto.RowKey, GameType = mapDto.GameType, PartitionKey = mapDto.GameType.ToString()};

            var operation = TableOperation.InsertOrMerge(mapEntity);
            await _mapsTable.ExecuteAsync(operation);
        }

        public async Task<int> GetMapListCount(MapsFilterModel filterModel)
        {
            if (filterModel == null) filterModel = new MapsFilterModel();

            return await filterModel.ApplyFilter(_legacyContext).CountAsync();
        }

        public async Task<List<MapsListEntryViewModel>> GetMapList(MapsFilterModel filterModel)
        {
            if (filterModel == null) filterModel = new MapsFilterModel();

            var maps = await filterModel.ApplyFilter(_legacyContext).Include(m => m.MapFiles).Include(m => m.MapVotes).ToListAsync();

            var mapsResult = new List<MapsListEntryViewModel>();

            foreach (var map in maps)
            {
                double totalLikes = map.MapVotes.Count(mv => mv.Like);
                double totalDislikes = map.MapVotes.Count(mv => !mv.Like);
                var totalVotes = map.MapVotes.Count();
                double likePercentage = 0;
                double dislikePercentage = 0;

                if (totalVotes > 0)
                {
                    likePercentage = totalLikes / totalVotes * 100;
                    dislikePercentage = totalDislikes / totalVotes * 100;
                }

                var mapListEntryViewModel = new MapsListEntryViewModel
                {
                    GameType = map.GameType.ToString(),
                    MapName = map.MapName,
                    TotalVotes = totalVotes,
                    TotalLikes = totalLikes,
                    TotalDislikes = totalDislikes,
                    LikePercentage = likePercentage,
                    DislikePercentage = dislikePercentage,
                    MapFiles = new Dictionary<string, string>()
                };

                foreach (var mapFile in map.MapFiles) mapListEntryViewModel.MapFiles.Add(mapFile.FileName, $"{_options.MapRedirectBaseUrl}/redirect/{map.GameType.ToRedirectShortName()}/usermaps/{map.MapName}/{mapFile.FileName}");

                mapsResult.Add(mapListEntryViewModel);
            }

            return mapsResult;
        }
    }
}