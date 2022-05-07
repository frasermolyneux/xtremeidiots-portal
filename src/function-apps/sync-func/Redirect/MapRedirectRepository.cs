﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using XtremeIdiots.Portal.SyncFunc.Models;

namespace XtremeIdiots.Portal.SyncFunc.Redirect
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