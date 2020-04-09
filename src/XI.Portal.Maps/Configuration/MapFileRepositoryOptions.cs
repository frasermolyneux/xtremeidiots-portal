using System;

namespace XI.Portal.Maps.Configuration
{
    internal class MapFileRepositoryOptions : IMapFileRepositoryOptions
    {
        public string MapRedirectBaseUrl { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(MapRedirectBaseUrl))
                throw new NullReferenceException(nameof(MapRedirectBaseUrl));
        }
    }
}