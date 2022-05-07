using FM.AzureTableExtensions.Library;
using FM.AzureTableExtensions.Library.Attributes;
using FM.GeoLocation.Contract.Models;
using System;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;

namespace XI.Portal.Players.Models
{
    public class PlayerLocationEntity : TableEntityExtended
    {
        [EntityEnumPropertyConverter] public GameType GameType { get; set; }

        public Guid ServerId { get; set; }
        public string ServerName { get; set; }
        public string Guid { get; set; }
        public string PlayerName { get; set; }

        [EntityJsonPropertyConverter] public GeoLocationDto GeoLocation { get; set; }
    }
}