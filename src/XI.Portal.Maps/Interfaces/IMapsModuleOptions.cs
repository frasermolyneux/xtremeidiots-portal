using System;

namespace XI.Portal.Maps.Interfaces
{
    public interface IMapsModuleOptions
    {
        Action<IMapImageRepositoryOptions> MapImageRepositoryOptions { get; set; }
        Action<IMapRedirectRepositoryOptions> MapRedirectRepositoryOptions { get; set; }
    }
}