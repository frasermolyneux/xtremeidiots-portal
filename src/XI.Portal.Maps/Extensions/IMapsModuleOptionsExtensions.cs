using System;
using XI.Portal.Maps.Interfaces;

namespace XI.Portal.Maps.Extensions
{
    public static class MapsModuleOptionsExtensions
    {
        public static void ConfigureMapFileRepository(this IMapsModuleOptions options, Action<IMapFileRepositoryOptions> repositoryOptions)
        {
            options.MapFileRepositoryOptions = repositoryOptions;
        }

        public static void ConfigureMapImageRepository(this IMapsModuleOptions options, Action<IMapImageRepositoryOptions> repositoryOptions)
        {
            options.MapImageRepositoryOptions = repositoryOptions;
        }

        public static void ConfigureMapsRepository(this IMapsModuleOptions options, Action<ILegacyMapsRepositoryOptions> repositoryOptions)
        {
            options.MapsRepositoryOptions = repositoryOptions;
        }

        public static void ConfigureMapRedirectRepository(this IMapsModuleOptions options, Action<IMapRedirectRepositoryOptions> repositoryOptions)
        {
            options.MapRedirectRepositoryOptions = repositoryOptions;
        }
    }
}