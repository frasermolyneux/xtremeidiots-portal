using System;

namespace XI.Portal.Demos.Configuration
{
    internal class DemoModuleOptions : IDemoModuleOptions
    {
        public Action<IDemoAuthRepositoryOptions> DemoAuthRepositoryOptions { get; set; }
        public Action<IDemosRepositoryOptions> DemosRepositoryOptions { get; set; }

        public void Validate()
        {
            if (DemoAuthRepositoryOptions == null)
                throw new NullReferenceException(nameof(DemoAuthRepositoryOptions));

            if (DemosRepositoryOptions == null)
                throw new NullReferenceException(nameof(DemosRepositoryOptions));
        }
    }
}