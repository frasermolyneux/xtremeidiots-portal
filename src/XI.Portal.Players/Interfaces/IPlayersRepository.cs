using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XI.CommonTypes;
using XI.Portal.Players.Dto;
using XI.Portal.Players.Models;

namespace XI.Portal.Players.Interfaces
{
    public interface IPlayersRepository
    {
        Task<int> GetPlayerListCount(PlayersFilterModel filterModel);
        Task<List<PlayerListEntryViewModel>> GetPlayerList(PlayersFilterModel filterModel);
        Task<PlayerDto> GetPlayer(Guid id);
        Task<PlayerDto> GetPlayer(GameType gameType, string guid);
        Task<List<AliasDto>> GetPlayerAliases(Guid id);
        Task<List<IpAddressDto>> GetPlayerIpAddresses(Guid id);
        Task<List<RelatedPlayerDto>> GetRelatedPlayers(Guid id, string ipAddress);
        Task CreatePlayer(PlayerDto playerDto);
    }
}