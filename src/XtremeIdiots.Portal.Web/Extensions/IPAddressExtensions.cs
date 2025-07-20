using Microsoft.AspNetCore.Html;
using System.Text;
using XtremeIdiots.Portal.Web.Services;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players;
using XtremeIdiots.Portal.Web.Models;
using MX.GeoLocation.Abstractions.Models.V1;

namespace XtremeIdiots.Portal.Web.Extensions
{

    public static class IPAddressExtensions
    {

        public static HtmlString FormatIPAddress(
            this string ipAddress,
            GeoLocationDto? geoLocation = null,
            int? riskScore = null,
            bool? isProxy = null,
            bool? isVpn = null,
            string? proxyType = null,
            bool linkToDetails = true)
        {
            if (string.IsNullOrEmpty(ipAddress))
                return HtmlString.Empty;

            var sb = new StringBuilder();

            if (geoLocation != null)
            {
                sb.Append(geoLocation.FlagImage().Value);
                sb.Append(' ');
            }
            else if (!string.IsNullOrEmpty(geoLocation?.CountryCode))
            {
                sb.Append(geoLocation.CountryCode.FlagImage().Value);
                sb.Append(' ');
            }
            else
            {

                sb.Append("<img src=\"/images/flags/unknown.png\" /> ");
            }

            if (linkToDetails)
            {
                sb.Append($"<a href=\"/IPAddresses/Details?ipAddress={ipAddress}\">{ipAddress}</a>");
            }
            else
            {
                sb.Append(ipAddress);
            }

            if (riskScore.HasValue)
            {
                string riskClass = GetRiskClass(riskScore.Value);
                sb.Append($" <span class=\"badge rounded-pill {riskClass}\">Risk: {riskScore}</span>");
            }

            if (!string.IsNullOrEmpty(proxyType))
            {
                sb.Append($" <span class=\"badge rounded-pill text-bg-primary\">{proxyType}</span>");
            }

            if (isProxy == true)
            {
                sb.Append(" <span class=\"badge rounded-pill text-bg-danger\">Proxy</span>");
            }

            if (isVpn == true)
            {
                sb.Append(" <span class=\"badge rounded-pill text-bg-warning\">VPN</span>");
            }

            return new HtmlString(sb.ToString());
        }

        public static HtmlString FormatIPAddress(
            this PlayerDto player,
            GeoLocationDto? geoLocation = null,
            bool linkToDetails = true)
        {
            if (player is null || string.IsNullOrEmpty(player.IpAddress))
                return HtmlString.Empty;

            return FormatIPAddress(
                player.IpAddress,
                geoLocation,
                player.ProxyCheckRiskScore(),
                player.IsProxy(),
                player.IsVpn(),
                player.ProxyType(),
                linkToDetails);
        }

        public static HtmlString FormatIPAddress(
            this string ipAddress,
            ProxyCheckResult proxyCheckResult,
            GeoLocationDto? geoLocation = null,
            bool linkToDetails = true)
        {
            if (string.IsNullOrEmpty(ipAddress) || proxyCheckResult is null || proxyCheckResult.IsError)
                return FormatIPAddress(ipAddress, geoLocation, null, null, null, null, linkToDetails);

            return FormatIPAddress(
                ipAddress,
                geoLocation,
                proxyCheckResult.RiskScore,
                proxyCheckResult.IsProxy,
                proxyCheckResult.IsVpn,
                proxyCheckResult.Type,
                linkToDetails);
        }

        private static string GetRiskClass(int riskScore)
        {
            return riskScore switch
            {
                >= 80 => "text-bg-danger",
                >= 50 => "text-bg-warning",
                >= 25 => "text-bg-info",
                _ => "text-bg-success"
            };
        }
    }
}