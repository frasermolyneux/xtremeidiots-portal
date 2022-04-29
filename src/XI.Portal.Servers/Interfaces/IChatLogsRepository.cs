using System;
using System.Threading.Tasks;
using XI.Portal.Servers.Dto;

namespace XI.Portal.Servers.Interfaces
{
    public interface IChatLogsRepository
    {
        Task<ChatLogDto> GetChatLog(Guid id);
    }
}