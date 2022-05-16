using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.ServersWebApi.Clients;
using XtremeIdiots.Portal.ServersWebApi.Interfaces;

namespace XtremeIdiots.Portal.ServersWebApi.Factories
{
    public class QueryClientFactory : IQueryClientFactory
    {
        private readonly ILogger<QueryClientFactory> _logger;

        public QueryClientFactory(ILogger<QueryClientFactory> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IQueryClient CreateInstance(GameType gameType, string hostname, int queryPort)
        {
            IQueryClient queryClient;

            switch (gameType)
            {
                case GameType.CallOfDuty2:
                case GameType.CallOfDuty4:
                case GameType.CallOfDuty5:
                    queryClient = new Quake3QueryClient(_logger);
                    break;
                case GameType.Insurgency:
                case GameType.Rust:
                case GameType.Left4Dead2:
                    queryClient = new SourceQueryClient(_logger);
                    break;
                default:
                    throw new Exception("Unsupported game type");
            }

            queryClient.Configure(hostname, queryPort);
            return queryClient;
        }
    }
}