using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players;

namespace XtremeIdiots.Portal.Web.Models
{

    public static class PlayerDtoExtensions
    {
        private static readonly ConcurrentDictionary<Guid, int> RiskScores = new ConcurrentDictionary<Guid, int>();
        private static readonly ConcurrentDictionary<Guid, bool> IsProxyFlags = new ConcurrentDictionary<Guid, bool>();
        private static readonly ConcurrentDictionary<Guid, bool> IsVpnFlags = new ConcurrentDictionary<Guid, bool>();
        private static readonly ConcurrentDictionary<Guid, string> ProxyTypes = new ConcurrentDictionary<Guid, string>();

        public static int ProxyCheckRiskScore(this PlayerDto playerDto)
        {
            if (playerDto is null)
                return 0;

            return RiskScores.TryGetValue(playerDto.PlayerId, out var score) ? score : 0;
        }

        public static void SetProxyCheckRiskScore(this PlayerDto playerDto, int value)
        {
            if (playerDto is null)
                return;

            RiskScores.AddOrUpdate(playerDto.PlayerId, value, (_, _) => value);
        }

        public static bool IsProxy(this PlayerDto playerDto)
        {
            if (playerDto is null)
                return false;

            return IsProxyFlags.TryGetValue(playerDto.PlayerId, out var isProxy) && isProxy;
        }

        public static void SetIsProxy(this PlayerDto playerDto, bool value)
        {
            if (playerDto is null)
                return;

            IsProxyFlags.AddOrUpdate(playerDto.PlayerId, value, (_, _) => value);
        }

        public static bool IsVpn(this PlayerDto playerDto)
        {
            if (playerDto is null)
                return false;

            return IsVpnFlags.TryGetValue(playerDto.PlayerId, out var isVpn) && isVpn;
        }

        public static void SetIsVpn(this PlayerDto playerDto, bool value)
        {
            if (playerDto is null)
                return;

            IsVpnFlags.AddOrUpdate(playerDto.PlayerId, value, (_, _) => value);
        }

        public static string ProxyType(this PlayerDto playerDto)
        {
            if (playerDto is null)
                return string.Empty;

            return ProxyTypes.TryGetValue(playerDto.PlayerId, out var type) ? type : string.Empty;
        }

        public static void SetProxyType(this PlayerDto playerDto, string value)
        {
            if (playerDto is null)
                return;

            ProxyTypes.AddOrUpdate(playerDto.PlayerId, value, (_, _) => value);
        }
    }
}