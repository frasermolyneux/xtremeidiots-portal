using System;

namespace XI.Portal.Maps.Configuration
{
    public interface IMapsModuleOptions
    {
        Action<IMapFileRepositoryOptions> MapFileRepositoryOptions { get; set; }
        Action<IMapImageRepositoryOptions> MapImageRepositoryOptions { get; set; }
        Action<IMapsRepositoryOptions> MapsRepositoryOptions { get; set; }


        void Validate();
    }
}