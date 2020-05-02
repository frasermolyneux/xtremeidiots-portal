using System;
using XI.Portal.Maps.Interfaces;

namespace XI.Portal.Maps.Configuration
{
    internal class MapsRepositoryOptions : IMapsRepositoryOptions
    {
        public string MapRedirectBaseUrl { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(MapRedirectBaseUrl))
                throw new NullReferenceException(nameof(MapRedirectBaseUrl));
        }
    }
}