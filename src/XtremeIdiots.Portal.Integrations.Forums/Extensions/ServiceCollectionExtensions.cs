using Microsoft.Extensions.DependencyInjection;

namespace XtremeIdiots.Portal.Integrations.Forums.Extensions;

/// <summary>
/// Extension methods for configuring forum integration services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds forum integration services for admin action topics and demo management
    /// </summary>
    /// <param name="serviceCollection">The service collection to configure</param>
    public static void AddAdminActionTopics(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IAdminActionTopics, AdminActionTopics>();
        serviceCollection.AddSingleton<IDemoManager, DemoManager>();
    }
}