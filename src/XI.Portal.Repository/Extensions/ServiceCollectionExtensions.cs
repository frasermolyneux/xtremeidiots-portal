using Microsoft.Extensions.DependencyInjection;
using XI.Portal.Repository.Interfaces;

namespace XI.Portal.Repository.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddAppData(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IMapVotesRepository, MapVotesRepository>();
        }
    }
}