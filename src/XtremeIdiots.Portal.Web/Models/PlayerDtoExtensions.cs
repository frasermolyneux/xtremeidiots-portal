using System.Collections.Concurrent;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players;

namespace XtremeIdiots.Portal.Web.Models;

public static class PlayerDtoExtensions
{
    private readonly static ConcurrentDictionary<Guid, int> riskScores = new();
    private readonly static ConcurrentDictionary<Guid, bool> isProxyFlags = new();
    private readonly static ConcurrentDictionary<Guid, bool> isVpnFlags = new();
    private readonly static ConcurrentDictionary<Guid, string> proxyTypes = new();

    public static int ProxyCheckRiskScore(this PlayerDto playerDto)
    {
        return playerDto is null ? 0 : riskScores.TryGetValue(playerDto.PlayerId, out var score) ? score : 0;
    }

    public static void SetProxyCheckRiskScore(this PlayerDto playerDto, int value)
    {
        if (playerDto is null)
            return;

        riskScores.AddOrUpdate(playerDto.PlayerId, value, (_, _) => value);
    }

    public static bool IsProxy(this PlayerDto playerDto)
    {
        return playerDto is not null && isProxyFlags.TryGetValue(playerDto.PlayerId, out var isProxy) && isProxy;
    }

    public static void SetIsProxy(this PlayerDto playerDto, bool value)
    {
        if (playerDto is null)
            return;

        isProxyFlags.AddOrUpdate(playerDto.PlayerId, value, (_, _) => value);
    }

    public static bool IsVpn(this PlayerDto playerDto)
    {
        return playerDto is not null && isVpnFlags.TryGetValue(playerDto.PlayerId, out var isVpn) && isVpn;
    }

    public static void SetIsVpn(this PlayerDto playerDto, bool value)
    {
        if (playerDto is null)
            return;

        isVpnFlags.AddOrUpdate(playerDto.PlayerId, value, (_, _) => value);
    }

    public static string ProxyType(this PlayerDto playerDto)
    {
        return playerDto is null ? string.Empty : proxyTypes.TryGetValue(playerDto.PlayerId, out var type) ? type : string.Empty;
    }

    public static void SetProxyType(this PlayerDto playerDto, string value)
    {
        if (playerDto is null)
            return;

        proxyTypes.AddOrUpdate(playerDto.PlayerId, value, (_, _) => value);
    }
}