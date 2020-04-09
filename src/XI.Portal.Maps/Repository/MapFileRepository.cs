using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using XI.Portal.Data.Legacy;
using XI.Portal.Maps.Configuration;
using XI.Portal.Maps.Extensions;

namespace XI.Portal.Maps.Repository
{
    public class MapFileRepository : IMapFileRepository
    {
        private readonly IMapFileRepositoryOptions _options;
        private readonly LegacyPortalContext _legacyContext;

        public MapFileRepository(IMapFileRepositoryOptions options, LegacyPortalContext legacyContext)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));
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

        private string GetTemporaryDirectory()
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }
    }
}
