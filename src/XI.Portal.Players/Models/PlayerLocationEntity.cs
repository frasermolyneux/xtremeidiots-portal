using System;
using FM.GeoLocation.Contract.Models;
using XI.AzureTableExtensions;
using XI.AzureTableExtensions.Attributes;
using XI.CommonTypes;

namespace XI.Portal.Players.Models
{
    public class PlayerLocationEntity : TableEntityExtended
    {
        [EntityEnumPropertyConverter] public GameType GameType { get; set; }

        public Guid ServerId { get; set; }
        public string ServerName { get; set; }
        public Guid PlayerId { get; set; }
        public string PlayerName { get; set; }

        [EntityJsonPropertyConverter] public GeoLocationDto GeoLocation { get; set; }
    }
}