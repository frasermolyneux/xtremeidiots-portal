using System;
using XI.Portal.Maps.Interfaces;

namespace XI.Portal.Maps.Configuration
{
    internal class MapsModuleOptions : IMapsModuleOptions
    {
        public Action<IMapFileRepositoryOptions> MapFileRepositoryOptions { get; set; }
        public Action<IMapImageRepositoryOptions> MapImageRepositoryOptions { get; set; }
        public Action<ILegacyMapsRepositoryOptions> MapsRepositoryOptions { get; set; }
        public Action<IMapRedirectRepositoryOptions> MapRedirectRepositoryOptions { get; set; }
    }
}