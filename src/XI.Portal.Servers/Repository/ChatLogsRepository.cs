using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using XI.Portal.Data.Legacy;
using XI.Portal.Servers.Dto;
using XI.Portal.Servers.Interfaces;

namespace XI.Portal.Servers.Repository
{
    public class ChatLogsRepository : IChatLogsRepository
    {
        private readonly LegacyPortalContext _legacyContext;

        public ChatLogsRepository(LegacyPortalContext legacyContext)
        {
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));
        }

        public async Task<ChatLogDto> GetChatLog(Guid id)
        {
            var chatLog = await _legacyContext.ChatLogs.Include(cl => cl.GameServerServer).SingleAsync(cl => cl.ChatLogId == id);

            return new ChatLogDto
            {
                ChatLogId = chatLog.ChatLogId,
                PlayerId = chatLog.PlayerPlayerId,
                ServerId = chatLog.GameServerServerId,
                ServerName = chatLog.GameServerServer.Title,
                GameType = chatLog.GameServerServer.GameType.ToString(),
                Timestamp = chatLog.Timestamp,
                Username = chatLog.Username,
                ChatType = chatLog.ChatType.ToString(),
                Message = chatLog.Message
            };
        }
    }
}