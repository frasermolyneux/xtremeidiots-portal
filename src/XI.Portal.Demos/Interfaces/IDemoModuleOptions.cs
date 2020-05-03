using System;

namespace XI.Portal.Demos.Interfaces
{
    public interface IDemoModuleOptions
    {
        Action<IDemoAuthRepositoryOptions> DemoAuthRepositoryOptions { get; set; }
        Action<IDemosRepositoryOptions> DemosRepositoryOptions { get; set; }
    }
}