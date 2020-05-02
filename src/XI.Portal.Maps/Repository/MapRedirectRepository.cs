using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using XI.Portal.Maps.Interfaces;
using XI.Portal.Maps.Models;

namespace XI.Portal.Maps.Repository
{
    public class MapRedirectRepository : IMapRedirectRepository
    {
        private readonly IMapRedirectRepositoryOptions _options;

        public MapRedirectRepository(IMapRedirectRepositoryOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public List<MapRedirectEntry> GetMapEntriesForGame(string game)
        {
            using (var client = new WebClient())
            {
                var content = client.DownloadString($"{_options.MapRedirectBaseUrl}/portal-map-sync.php?game={game}&key={_options.ApiKey}");
                return JsonConvert.DeserializeObject<List<MapRedirectEntry>>(content);
            }
        }
    }
}