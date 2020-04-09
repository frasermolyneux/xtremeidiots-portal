using System;

namespace XI.Portal.Maps.Configuration
{
    internal class MapsModuleOptions : IMapsModuleOptions
    {
        public Action<IMapFileRepositoryOptions> MapFileRepositoryOptions { get; set; }
        public Action<IMapImageRepositoryOptions> MapImageRepositoryOptions { get; set; }
        public Action<IMapsRepositoryOptions> MapsRepositoryOptions { get; set; }

        public void Validate()
        {
            if (MapsRepositoryOptions == null)
                throw new NullReferenceException(nameof(MapsRepositoryOptions));
        }
    }
}