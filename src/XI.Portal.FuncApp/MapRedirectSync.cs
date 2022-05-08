using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using XI.Portal.Repository.Interfaces;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.Providers;

// ReSharper disable StringLiteralTypo

namespace XI.Portal.FuncApp
{
    // ReSharper disable once UnusedMember.Global
    public class MapRedirectSync
    {
        private readonly IMapsRepository _mapsRepository;
        private readonly IRepositoryTokenProvider repositoryTokenProvider;
        private readonly IRepositoryApiClient repositoryApiClient;

        public MapRedirectSync(
            IMapsRepository mapsRepository,
            IRepositoryTokenProvider repositoryTokenProvider,
            IRepositoryApiClient repositoryApiClient)
        {
            _mapsRepository = mapsRepository ?? throw new ArgumentNullException(nameof(mapsRepository));
            this.repositoryTokenProvider = repositoryTokenProvider;
            this.repositoryApiClient = repositoryApiClient;
        }

        [FunctionName("MapVoteTransfer")]
        public async Task RunMapVoteTransfer([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            var mapVotes = await _mapsRepository.GetMapVotes();

            foreach (var gameType in new[] { GameType.CallOfDuty2, GameType.CallOfDuty4, GameType.CallOfDuty5 })
            {
                var gameMapVotes = mapVotes.Where(mv => mv.PartitionKey == gameType.ToString()).ToList();
                var maps = gameMapVotes.Select(mv => mv.MapName).Distinct().ToList();

                foreach (var map in maps)
                {
                    var mapDto = await repositoryApiClient.Maps.GetMap(accessToken, gameType, map);

                    foreach (var mapVote in gameMapVotes)
                    {
                        var player = await repositoryApiClient.Players.GetPlayerByGameType(accessToken, gameType, mapVote.Guid);

                        if (player != null)
                        {
                            await repositoryApiClient.Maps.UpsertMapVote(accessToken, mapDto.MapId, player.Id, mapVote.Like);
                        }
                    }
                }
            }
        }
    }
}