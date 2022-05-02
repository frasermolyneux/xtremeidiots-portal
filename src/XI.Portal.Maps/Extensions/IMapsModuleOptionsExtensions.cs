using System;
using XI.Portal.Maps.Interfaces;

namespace XI.Portal.Maps.Extensions
{
    public static class MapsModuleOptionsExtensions
    {
        public static void ConfigureMapImageRepository(this IMapsModuleOptions options, Action<IMapImageRepositoryOptions> repositoryOptions)
        {
            options.MapImageRepositoryOptions = repositoryOptions;
        }

        public static void ConfigureMapRedirectRepository(this IMapsModuleOptions options, Action<IMapRedirectRepositoryOptions> repositoryOptions)
        {
            options.MapRedirectRepositoryOptions = repositoryOptions;
        }
    }
}