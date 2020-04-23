using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using XI.Portal.Players.Dto;
using XI.Portal.Players.Models;

namespace XI.Portal.Players.Interfaces
{
    public interface IPlayersRepository
    {
        Task<int> GetPlayerListCount(PlayersFilterModel filterModel);
        Task<List<PlayerListEntryViewModel>> GetPlayerList(PlayersFilterModel filterModel);
        Task<PlayerDto> GetPlayer(Guid id, ClaimsPrincipal user, string[] requiredClaims);
        Task<List<AliasDto>> GetPlayerAliases(Guid id, ClaimsPrincipal user, string[] requiredClaims);
        Task<List<IpAddressDto>> GetPlayerIpAddresses(Guid id, ClaimsPrincipal user, string[] requiredClaims);
        Task<List<RelatedPlayerDto>> GetRelatedPlayers(Guid id, string ipAddress, ClaimsPrincipal user, string[] requiredClaims);
    }
}