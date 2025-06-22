using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using XtremeIdiots.Portal.AdminWebApp.Models;
using XtremeIdiots.Portal.AdminWebApp.Services;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;

namespace XtremeIdiots.Portal.AdminWebApp.Extensions
{
    /// <summary>
    /// Extension methods for enriching DTOs with ProxyCheck data.
    /// </summary>
    public static class ProxyCheckExtensions
    {
        /// <summary>
        /// Enriches a player DTO with ProxyCheck data for its IP address.
        /// </summary>
        /// <param name="playerDto">The player DTO to enrich.</param>
        /// <param name="proxyCheckService">The ProxyCheck service.</param>
        /// <param name="logger">Logger instance.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The player DTO enriched with ProxyCheck data.</returns>
        public static async Task<PlayerDto> EnrichWithProxyCheckDataAsync(
            this PlayerDto playerDto,
            IProxyCheckService proxyCheckService,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            if (playerDto == null)
                return playerDto!;

            if (string.IsNullOrEmpty(playerDto.IpAddress))
                return playerDto;

            try
            {
                var proxyCheckResult = await proxyCheckService.GetIpRiskDataAsync(playerDto.IpAddress, cancellationToken);
                if (!proxyCheckResult.IsError)
                {                    // Use the extension methods to store ProxyCheck data
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

        /// <summary>
        /// Enriches a collection of player DTOs with ProxyCheck data for their IP addresses.
        /// </summary>
        /// <param name="playerDtos">The collection of player DTOs to enrich.</param>
        /// <param name="proxyCheckService">The ProxyCheck service.</param>
        /// <param name="logger">Logger instance.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The collection of player DTOs enriched with ProxyCheck data.</returns>
        public static async Task<IEnumerable<PlayerDto>> EnrichWithProxyCheckDataAsync(
            this IEnumerable<PlayerDto> playerDtos,
            IProxyCheckService proxyCheckService,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            if (playerDtos == null)
                return Enumerable.Empty<PlayerDto>();

            var players = playerDtos.ToList();

            foreach (var player in players.Where(p => p != null))
            {
                await EnrichWithProxyCheckDataAsync(player, proxyCheckService, logger, cancellationToken);
            }

            return players;
        }
    }
}
