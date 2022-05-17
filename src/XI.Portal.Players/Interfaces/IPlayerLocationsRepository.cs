﻿using XI.Portal.Players.Dto;

namespace XI.Portal.Players.Interfaces
{
    public interface IPlayerLocationsRepository
    {
        Task<List<PlayerLocationDto>> GetLocations();
        Task UpdateEntry(PlayerLocationDto model);
        Task RemoveOldEntries(List<Guid> serverIds);
    }
}