﻿
using MX.GeoLocation.LookupApi.Abstractions.Models;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;

namespace XtremeIdiots.Portal.AdminWebApp.ViewModels
{
    public class PlayerDetailsViewModel
    {
        public PlayerDto Player { get; set; }
        public GeoLocationDto GeoLocation { get; set; }
    }
}
