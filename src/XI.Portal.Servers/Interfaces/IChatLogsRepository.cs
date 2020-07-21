using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XI.Portal.Servers.Dto;
using XI.Portal.Servers.Models;

namespace XI.Portal.Servers.Interfaces
{
    public interface IChatLogsRepository
    {
        Task<int> GetChatLogCount(ChatLogFilterModel filterModel);
        Task<List<ChatLogDto>> GetChatLogs(ChatLogFilterModel filterModel);
        Task<ChatLogDto> GetChatLog(Guid id);
        Task CreateChatLog(ChatLogDto chatLogDto);
    }
}