using System.Collections.Generic;
using System.Threading.Tasks;
using XI.Portal.Players.Models;

namespace XI.Portal.Players.Interfaces
{
    public interface IPlayersRepository
    {
        Task<int> GetPlayerListCount(PlayersFilterModel filterModel = null);
        Task<List<PlayerListEntryViewModel>> GetPlayerList(PlayersFilterModel filterModel = null);
    }
}