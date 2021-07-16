using Microsoft.Extensions.DependencyInjection;
using XI.Portal.Bus.Client;

namespace XI.Portal.Bus.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddServiceBus(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IPortalServiceBusClient, PortalServiceBusClient>();
        }
    }
}