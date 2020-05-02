using System;
using XI.Portal.Maps.Configuration;

namespace XI.Portal.Maps.Interfaces
{
    public interface IMapsModuleOptions
    {
        Action<IMapFileRepositoryOptions> MapFileRepositoryOptions { get; set; }
        Action<IMapImageRepositoryOptions> MapImageRepositoryOptions { get; set; }
        Action<IMapsRepositoryOptions> MapsRepositoryOptions { get; set; }
        Action<IMapRedirectRepositoryOptions> MapRedirectRepositoryOptions { get; set; }

        void Validate();
    }
}