using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players;

namespace XtremeIdiots.Portal.Web.Models
{
    /// <summary>
    /// Extension properties for PlayerDto to include ProxyCheck data.
    /// </summary>
    public static class PlayerDtoExtensions
    {        // Use static dictionaries to store ProxyCheck data by player ID
        private static readonly ConcurrentDictionary<Guid, int> RiskScores = new ConcurrentDictionary<Guid, int>();
        private static readonly ConcurrentDictionary<Guid, bool> IsProxyFlags = new ConcurrentDictionary<Guid, bool>();
        private static readonly ConcurrentDictionary<Guid, bool> IsVpnFlags = new ConcurrentDictionary<Guid, bool>();
        private static readonly ConcurrentDictionary<Guid, string> ProxyTypes = new ConcurrentDictionary<Guid, string>();

        /// <summary>
        /// Gets the ProxyCheck risk score.
        /// </summary>
        public static int ProxyCheckRiskScore(this PlayerDto playerDto)
        {
            if (playerDto is null)
                return 0;

            return RiskScores.TryGetValue(playerDto.PlayerId, out var score) ? score : 0;
        }

        /// <summary>
        /// Sets the ProxyCheck risk score.
        /// </summary>
        public static void SetProxyCheckRiskScore(this PlayerDto playerDto, int value)
        {
            if (playerDto is null)
                return;

            RiskScores.AddOrUpdate(playerDto.PlayerId, value, (_, _) => value);
        }

        /// <summary>
        /// Gets whether the IP is a proxy.
        /// </summary>
        public static bool IsProxy(this PlayerDto playerDto)
        {
            if (playerDto is null)
                return false;

            return IsProxyFlags.TryGetValue(playerDto.PlayerId, out var isProxy) && isProxy;
        }

        /// <summary>
        /// Sets whether the IP is a proxy.
        /// </summary>
        public static void SetIsProxy(this PlayerDto playerDto, bool value)
        {
            if (playerDto is null)
                return;

            IsProxyFlags.AddOrUpdate(playerDto.PlayerId, value, (_, _) => value);
        }

        /// <summary>
        /// Gets whether the IP is a VPN.
        /// </summary>
        public static bool IsVpn(this PlayerDto playerDto)
        {
            if (playerDto is null)
                return false;

            return IsVpnFlags.TryGetValue(playerDto.PlayerId, out var isVpn) && isVpn;
        }

        /// <summary>
        /// Sets whether the IP is a VPN.
        /// </summary>
        public static void SetIsVpn(this PlayerDto playerDto, bool value)
        {
            if (playerDto is null)
                return;

            IsVpnFlags.AddOrUpdate(playerDto.PlayerId, value, (_, _) => value);
        }

        /// <summary>
        /// Gets the proxy type.
        /// </summary>
        public static string ProxyType(this PlayerDto playerDto)
        {
            if (playerDto is null)
                return string.Empty;

            return ProxyTypes.TryGetValue(playerDto.PlayerId, out var type) ? type : string.Empty;
        }

        /// <summary>
        /// Sets the proxy type.
        /// </summary>
        public static void SetProxyType(this PlayerDto playerDto, string value)
        {
            if (playerDto is null)
                return;

            ProxyTypes.AddOrUpdate(playerDto.PlayerId, value, (_, _) => value);
        }
    }
}
