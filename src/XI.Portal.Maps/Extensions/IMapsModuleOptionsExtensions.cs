using System;
using XI.Portal.Maps.Configuration;

namespace XI.Portal.Maps.Extensions
{
    public static class MapsModuleOptionsExtensions
    {
        public static void UseMapFileRepository(this IMapsModuleOptions options, Action<IMapFileRepositoryOptions> repositoryOptions)
        {
            options.MapFileRepositoryOptions = repositoryOptions;
        }

        public static void UseMapImageRepository(this IMapsModuleOptions options, Action<IMapImageRepositoryOptions> repositoryOptions)
        {
            options.MapImageRepositoryOptions = repositoryOptions;
        }

        public static void UseMapsRepository(this IMapsModuleOptions options, Action<IMapsRepositoryOptions> repositoryOptions)
        {
            options.MapsRepositoryOptions = repositoryOptions;
        }
    }
}