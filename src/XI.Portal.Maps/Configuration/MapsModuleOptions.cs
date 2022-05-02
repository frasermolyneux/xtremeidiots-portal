using System;
using XI.Portal.Maps.Interfaces;

namespace XI.Portal.Maps.Configuration
{
    internal class MapsModuleOptions : IMapsModuleOptions
    {
        public Action<IMapImageRepositoryOptions> MapImageRepositoryOptions { get; set; }
        public Action<IMapRedirectRepositoryOptions> MapRedirectRepositoryOptions { get; set; }
    }
}