using System;
using XI.Portal.Demos.Interfaces;

namespace XI.Portal.Demos.Extensions
{
    public static class DemoModuleOptionsExtensions
    {
        public static void ConfigureDemosRepository(this IDemoModuleOptions options, Action<IDemosRepositoryOptions> repositoryOptions)
        {
            options.DemosRepositoryOptions = repositoryOptions;
        }

        public static void ConfigureDemoAuthRepository(this IDemoModuleOptions options, Action<IDemoAuthRepositoryOptions> repositoryOptions)
        {
            options.DemoAuthRepositoryOptions = repositoryOptions;
        }
    }
}