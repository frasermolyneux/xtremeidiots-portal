using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using XI.Portal.Data.Legacy;
using XI.Portal.Data.Legacy.CommonTypes;
using XI.Portal.Maps.Configuration;
using XI.Portal.Maps.Extensions;
using XI.Portal.Maps.Models;
using XI.Portal.Maps.Properties;

namespace XI.Portal.Maps.Repository
{
    public class MapsRepository : IMapsRepository
    {
        private readonly LegacyPortalContext _legacyContext;
        private readonly IMapsOptions _options;

        public MapsRepository(IMapsOptions options, LegacyPortalContext legacyContext)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));
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

        public async Task<byte[]> GetFullRotationArchive(Guid id)
        {
            var gameServer = await _legacyContext.GameServers
                .SingleOrDefaultAsync(server => server.ServerId == id);

            var mapRotation = await _legacyContext.MapRotations
                .Include(m => m.MapMap)
                .Include(m => m.MapMap.MapFiles)
                .Include(m => m.MapMap.MapVotes)
                .Where(m => m.GameServerServer.ServerId == gameServer.ServerId).ToListAsync();

            var tempDirectory = GetTemporaryDirectory();

            using (var client = new WebClient())
            {
                foreach (var mapEntry in mapRotation)
                foreach (var mapFile in mapEntry.MapMap.MapFiles)
                {
                    var dir = Path.Combine(tempDirectory, mapEntry.MapMap.MapName);

                    Directory.CreateDirectory(dir);

                    client.DownloadFile(new Uri($"{_options.MapRedirectBaseUrl}/redirect/{mapEntry.MapMap.GameType.ToRedirectShortName()}/usermaps/{mapEntry.MapMap.MapName}/{mapFile.FileName}"),
                        Path.Combine(dir, mapFile.FileName));
                }
            }

            var zippedMapPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            ZipFile.CreateFromDirectory(tempDirectory, zippedMapPath);

            return File.ReadAllBytes(zippedMapPath);
        }

        public async Task<byte[]> GetMapImage(GameType gameType, string mapName)
        {
            try
            {
                var mapFilePath = Path.Combine(GetTemporaryDirectory(), $"{gameType}_{mapName}.jpg");
                var gameTrackerImageUrl = $"https://image.gametracker.com/images/maps/160x120/{gameType.ToGameTrackerShortName()}/{mapName}.jpg";

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                using (var client = new WebClient())
                {
                    client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36");
                    client.DownloadFile(new Uri(gameTrackerImageUrl), mapFilePath);
                }

                return File.ReadAllBytes(mapFilePath);
            }
            catch
            {
                return Resources.noimage;
            }
        }

        private string GetTemporaryDirectory()
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }
    }
}