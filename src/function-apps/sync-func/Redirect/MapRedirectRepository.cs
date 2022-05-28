using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using XtremeIdiots.Portal.SyncFunc.Models;

namespace XtremeIdiots.Portal.SyncFunc.Redirect
{
    public class MapRedirectRepository : IMapRedirectRepository
    {
        private readonly IOptions<MapRedirectRepositoryOptions> _options;

        public MapRedirectRepository(IOptions<MapRedirectRepositoryOptions> options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public List<MapRedirectEntry> GetMapEntriesForGame(string game)
        {
            using (var client = new WebClient())
            {
                var content = client.DownloadString($"{_options.Value.MapRedirectBaseUrl}/portal-map-sync.php?game={game}&key={_options.Value.ApiKey}");
                return JsonConvert.DeserializeObject<List<MapRedirectEntry>>(content);
            }
        }
    }
}