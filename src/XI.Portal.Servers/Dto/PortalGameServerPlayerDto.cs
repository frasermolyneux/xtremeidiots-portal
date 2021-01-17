using FM.AzureTableExtensions.Library.Attributes;
using FM.GeoLocation.Contract.Models;

namespace XI.Portal.Servers.Dto
{
    public class PortalGameServerPlayerDto
    {
        public string Num { get; set; }
        public string Guid { get; set; }
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public int Score { get; set; }
        public string Rate { get; set; }

        [EntityJsonPropertyConverter] public GeoLocationDto GeoLocation { get; set; }
    }
}