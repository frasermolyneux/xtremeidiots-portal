using System;
using XI.Portal.Demos.Interfaces;

namespace XI.Portal.Demos.Configuration
{
    internal class DemoModuleOptions : IDemoModuleOptions
    {
        public Action<IDemoAuthRepositoryOptions> DemoAuthRepositoryOptions { get; set; }
        public Action<IDemosRepositoryOptions> DemosRepositoryOptions { get; set; }
    }
}