using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players;
using XtremeIdiots.Portal.Web.Models;
using XtremeIdiots.Portal.Web.Services;

namespace XtremeIdiots.Portal.Web.Extensions;

public static class ProxyCheckExtensions
{

    public async static Task<PlayerDto> EnrichWithProxyCheckDataAsync(
        this PlayerDto playerDto,
        IProxyCheckService proxyCheckService,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        if (playerDto is null)
            return playerDto!;

        if (string.IsNullOrEmpty(playerDto.IpAddress))
            return playerDto;

        try
        {
            var proxyCheckResult = await proxyCheckService.GetIpRiskDataAsync(playerDto.IpAddress, cancellationToken);
            if (!proxyCheckResult.IsError)
            {
                PlayerDtoExtensions.SetProxyCheckRiskScore(playerDto, proxyCheckResult.RiskScore);
                PlayerDtoExtensions.SetIsProxy(playerDto, proxyCheckResult.IsProxy);
                PlayerDtoExtensions.SetIsVpn(playerDto, proxyCheckResult.IsVpn);
                PlayerDtoExtensions.SetProxyType(playerDto, proxyCheckResult.Type);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to enrich player DTO with ProxyCheck data for IP {IpAddress}", playerDto.IpAddress);
        }

        return playerDto;
    }

    public async static Task<IEnumerable<PlayerDto>> EnrichWithProxyCheckDataAsync(
        this IEnumerable<PlayerDto> playerDtos,
        IProxyCheckService proxyCheckService,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        if (playerDtos is null)
            return [];

        var players = playerDtos.ToList();

        foreach (var player in players.Where(p => p != null))
        {
            await EnrichWithProxyCheckDataAsync(player, proxyCheckService, logger, cancellationToken);
        }

        return players;
    }
}