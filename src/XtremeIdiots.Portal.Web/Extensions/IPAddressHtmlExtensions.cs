using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using MX.GeoLocation.Abstractions.Models.V1;

using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players;

namespace XtremeIdiots.Portal.Web.Extensions;

public static class IPAddressHtmlExtensions
{

    public static HtmlString FormatIPAddress(
        this IHtmlHelper html,
        string ipAddress,
        GeoLocationDto? geoLocation = null,
        int? riskScore = null,
        bool? isProxy = null,
        bool? isVpn = null,
        string? proxyType = null,
        bool linkToDetails = true)
    {
        return ipAddress.FormatIPAddress(geoLocation, riskScore, isProxy, isVpn, proxyType, linkToDetails);
    }

    public static HtmlString FormatIPAddress(
        this IHtmlHelper html,
        PlayerDto player,
        GeoLocationDto? geoLocation = null,
        bool linkToDetails = true)
    {
        return player.FormatIPAddress(geoLocation, linkToDetails);
    }
}