using System;

namespace XI.Portal.Demos.Interfaces
{
    public interface IDemoModuleOptions
    {
        Action<IDemosRepositoryOptions> DemosRepositoryOptions { get; set; }
    }
}