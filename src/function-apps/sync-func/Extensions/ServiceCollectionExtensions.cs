using Microsoft.Extensions.DependencyInjection;
using System;
using XtremeIdiots.Portal.SyncFunc.Ingest;
using XtremeIdiots.Portal.SyncFunc.Interfaces;
using XtremeIdiots.Portal.SyncFunc.Repository;
using XtremeIdiots.Portal.SyncFunc.Validators;

namespace XtremeIdiots.Portal.SyncFunc.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddBanFilesRepository(this IServiceCollection serviceCollection, Action<IBanFilesRepositoryOptions> options)
        {
            serviceCollection.Configure(options);

            serviceCollection.AddScoped<IBanFilesRepository, BanFilesRepository>();
            serviceCollection.AddScoped<IBanFileIngest, BanFileIngest>();
            serviceCollection.AddScoped<IGuidValidator, GuidValidator>();
        }
    }
}