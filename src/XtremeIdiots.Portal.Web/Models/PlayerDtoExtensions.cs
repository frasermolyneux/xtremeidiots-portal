using System.Collections.Concurrent;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players;

namespace XtremeIdiots.Portal.Web.Models;

/// <summary>
/// Extension methods for PlayerDto that provide proxy check functionality
/// </summary>
/// <remarks>
/// Uses in-memory caching for proxy check results to avoid repeated API calls
/// during a single request lifecycle
/// </remarks>
public static class PlayerDtoExtensions
{
    private readonly static ConcurrentDictionary<Guid, int> riskScores = [];
    private readonly static ConcurrentDictionary<Guid, bool> isProxyFlags = [];
    private readonly static ConcurrentDictionary<Guid, bool> isVpnFlags = [];
    private readonly static ConcurrentDictionary<Guid, string> proxyTypes = [];

    /// <summary>
    /// Gets the proxy check risk score for a player
    /// </summary>
    /// <param name="playerDto">The player DTO</param>
    /// <returns>Risk score from 0-100, or 0 if no score is cached</returns>
    public static int ProxyCheckRiskScore(this PlayerDto playerDto)
    {
        return playerDto is null ? 0 : riskScores.TryGetValue(playerDto.PlayerId, out var score) ? score : 0;
    }

    /// <summary>
    /// Sets the proxy check risk score for a player
    /// </summary>
    /// <param name="playerDto">The player DTO</param>
    /// <param name="value">Risk score from 0-100</param>
    public static void SetProxyCheckRiskScore(this PlayerDto playerDto, int value)
    {
        if (playerDto is null)
            return;

        riskScores.AddOrUpdate(playerDto.PlayerId, value, (_, _) => value);
    }

    /// <summary>
    /// Determines if a player's IP address is identified as a proxy
    /// </summary>
    /// <param name="playerDto">The player DTO</param>
    /// <returns>True if the IP is a proxy, false otherwise</returns>
    public static bool IsProxy(this PlayerDto playerDto)
    {
        return playerDto is not null && isProxyFlags.TryGetValue(playerDto.PlayerId, out var isProxy) && isProxy;
    }

    /// <summary>
    /// Sets the proxy status for a player
    /// </summary>
    /// <param name="playerDto">The player DTO</param>
    /// <param name="value">True if the IP is a proxy</param>
    public static void SetIsProxy(this PlayerDto playerDto, bool value)
    {
        if (playerDto is null)
            return;

        isProxyFlags.AddOrUpdate(playerDto.PlayerId, value, (_, _) => value);
    }

    /// <summary>
    /// Determines if a player's IP address is identified as a VPN
    /// </summary>
    /// <param name="playerDto">The player DTO</param>
    /// <returns>True if the IP is a VPN, false otherwise</returns>
    public static bool IsVpn(this PlayerDto playerDto)
    {
        return playerDto is not null && isVpnFlags.TryGetValue(playerDto.PlayerId, out var isVpn) && isVpn;
    }

    /// <summary>
    /// Sets the VPN status for a player
    /// </summary>
    /// <param name="playerDto">The player DTO</param>
    /// <param name="value">True if the IP is a VPN</param>
    public static void SetIsVpn(this PlayerDto playerDto, bool value)
    {
        if (playerDto is null)
            return;

        isVpnFlags.AddOrUpdate(playerDto.PlayerId, value, (_, _) => value);
    }

    /// <summary>
    /// Gets the proxy type classification for a player's IP address
    /// </summary>
    /// <param name="playerDto">The player DTO</param>
    /// <returns>Proxy type description, or empty string if not a proxy</returns>
    public static string ProxyType(this PlayerDto playerDto)
    {
        return playerDto is null ? string.Empty : proxyTypes.TryGetValue(playerDto.PlayerId, out var type) ? type : string.Empty;
    }

    /// <summary>
    /// Sets the proxy type classification for a player
    /// </summary>
    /// <param name="playerDto">The player DTO</param>
    /// <param name="value">The proxy type description</param>
    public static void SetProxyType(this PlayerDto playerDto, string value)
    {
        if (playerDto is null)
            return;

        proxyTypes.AddOrUpdate(playerDto.PlayerId, value, (_, _) => value);
    }
}