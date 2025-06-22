using Microsoft.AspNetCore.Html;
using System;
using System.Text;
using MX.GeoLocation.LookupApi.Abstractions.Models;
using XtremeIdiots.Portal.AdminWebApp.Services;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;
using XtremeIdiots.Portal.AdminWebApp.Models;

namespace XtremeIdiots.Portal.AdminWebApp.Extensions
{
    /// <summary>
    /// Extension methods for displaying IP addresses with consistent formatting.
    /// </summary>
    public static class IPAddressExtensions
    {
        /// <summary>
        /// Renders an IP address with consistent formatting following the pattern:
        /// {country flag} {IP Address} {VPN Pill} {Risk Pill} {Type Pill}
        /// </summary>
        /// <param name="ipAddress">The IP address to display</param>
        /// <param name="geoLocation">Optional GeoLocation data</param>
        /// <param name="riskScore">Optional risk score from ProxyCheck</param>
        /// <param name="isProxy">Optional flag indicating if the IP is a proxy</param>
        /// <param name="isVpn">Optional flag indicating if the IP is a VPN</param>
        /// <param name="proxyType">Optional proxy type</param>
        /// <param name="linkToDetails">Whether to link the IP to the details page</param>
        /// <returns>HTML formatted IP address</returns>
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

            // 1. Country Flag
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
                // Default flag for unknown country
                sb.Append("<img src=\"/images/flags/unknown.png\" /> ");
            }

            // 2. IP Address (with or without link)
            if (linkToDetails)
            {
                sb.Append($"<a href=\"/IPAddresses/Details?ipAddress={ipAddress}\">{ipAddress}</a>");
            }
            else
            {
                sb.Append(ipAddress);
            }

            // 3. Risk Pill (if available)
            if (riskScore.HasValue)
            {
                string riskClass = GetRiskClass(riskScore.Value);
                sb.Append($" <span class=\"badge rounded-pill {riskClass}\">Risk: {riskScore}</span>");
            }

            // 4. Type Pill (if available)
            if (!string.IsNullOrEmpty(proxyType))
            {
                sb.Append($" <span class=\"badge rounded-pill text-bg-primary\">{proxyType}</span>");
            }

            // 5. Proxy Pill
            if (isProxy == true)
            {
                sb.Append(" <span class=\"badge rounded-pill text-bg-danger\">Proxy</span>");
            }

            // 6. VPN Pill
            if (isVpn == true)
            {
                sb.Append(" <span class=\"badge rounded-pill text-bg-warning\">VPN</span>");
            }

            return new HtmlString(sb.ToString());
        }

        /// <summary>
        /// Renders an IP address for a player using the player's properties.
        /// </summary>
        /// <param name="player">The player DTO containing IP information</param>
        /// <param name="geoLocation">Optional GeoLocation data</param>
        /// <param name="linkToDetails">Whether to link the IP to the details page</param>
        /// <returns>HTML formatted IP address</returns>
        public static HtmlString FormatIPAddress(
            this PlayerDto player,
            GeoLocationDto? geoLocation = null,
            bool linkToDetails = true)
        {
            if (player == null || string.IsNullOrEmpty(player.IpAddress))
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

        /// <summary>
        /// Renders an IP address using data from a ProxyCheckResult.
        /// </summary>
        /// <param name="ipAddress">The IP address to display</param>
        /// <param name="proxyCheckResult">The ProxyCheck result</param>
        /// <param name="geoLocation">Optional GeoLocation data</param>
        /// <param name="linkToDetails">Whether to link the IP to the details page</param>
        /// <returns>HTML formatted IP address</returns>
        public static HtmlString FormatIPAddress(
            this string ipAddress,
            ProxyCheckResult proxyCheckResult,
            GeoLocationDto? geoLocation = null,
            bool linkToDetails = true)
        {
            if (string.IsNullOrEmpty(ipAddress) || proxyCheckResult == null || proxyCheckResult.IsError)
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

        /// <summary>
        /// Gets a CSS class based on the risk score for color-coding.
        /// </summary>
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
