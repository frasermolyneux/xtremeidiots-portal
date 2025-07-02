using Microsoft.Extensions.DependencyInjection;

namespace XtremeIdiots.Portal.Integrations.Forums.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddAdminActionTopics(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IAdminActionTopics, AdminActionTopics>();
            serviceCollection.AddSingleton<IDemoManager, DemoManager>();
        }
    }
}