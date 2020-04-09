using System;

namespace XI.Portal.Demos.Configuration
{
    public interface IDemoModuleOptions
    {
        Action<IDemoAuthRepositoryOptions> DemoAuthRepositoryOptions { get; set; }
        Action<IDemosRepositoryOptions> DemosRepositoryOptions { get; set; }

        void Validate();
    }
}