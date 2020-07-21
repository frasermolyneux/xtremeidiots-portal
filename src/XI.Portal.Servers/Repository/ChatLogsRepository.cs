using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using XI.CommonTypes;
using XI.Portal.Data.Legacy;
using XI.Portal.Data.Legacy.Models;
using XI.Portal.Servers.Dto;
using XI.Portal.Servers.Extensions;
using XI.Portal.Servers.Interfaces;
using XI.Portal.Servers.Models;

namespace XI.Portal.Servers.Repository
{
    public class ChatLogsRepository : IChatLogsRepository
    {
        private readonly LegacyPortalContext _legacyContext;

        public ChatLogsRepository(LegacyPortalContext legacyContext)
        {
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));
        }

        public async Task<int> GetChatLogCount(ChatLogFilterModel filterModel)
        {
            if (filterModel == null) filterModel = new ChatLogFilterModel();

            return await _legacyContext.ChatLogs.ApplyFilter(filterModel).CountAsync();
        }

        public async Task<List<ChatLogDto>> GetChatLogs(ChatLogFilterModel filterModel)
        {
            if (filterModel == null) filterModel = new ChatLogFilterModel();

            var chatLogs = await _legacyContext.ChatLogs.ApplyFilter(filterModel).ToListAsync();

            var results = new List<ChatLogDto>();

            foreach (var chatLog in chatLogs)
                results.Add(new ChatLogDto
                {
                    ChatLogId = chatLog.ChatLogId,
                    PlayerId = chatLog.PlayerPlayerId,
                    ServerId = chatLog.GameServerServerId,
                    GameType = chatLog.GameServerServer.GameType.ToString(),
                    Timestamp = chatLog.Timestamp,
                    Username = chatLog.Username,
                    ChatType = chatLog.ChatType.ToString(),
                    Message = chatLog.Message
                });

            return results;
        }

        public async Task<ChatLogDto> GetChatLog(Guid id)
        {
            var chatLog = await _legacyContext.ChatLogs.Include(cl => cl.GameServerServer).SingleAsync(cl => cl.ChatLogId == id);

            return new ChatLogDto
            {
                ChatLogId = chatLog.ChatLogId,
                PlayerId = chatLog.PlayerPlayerId,
                ServerId = chatLog.GameServerServerId,
                GameType = chatLog.GameServerServer.GameType.ToString(),
                Timestamp = chatLog.Timestamp,
                Username = chatLog.Username,
                ChatType = chatLog.ChatType.ToString(),
                Message = chatLog.Message
            };
        }

        public async Task CreateChatLog(ChatLogDto chatLogDto)
        {
            var player = await _legacyContext.Player2.SingleOrDefaultAsync(p => p.PlayerId == chatLogDto.PlayerId);
            var server = await _legacyContext.GameServers.SingleOrDefaultAsync(s => s.ServerId == chatLogDto.ServerId);

            var chatLog = new ChatLogs
            {
                GameServerServer = server,
                PlayerPlayer = player,
                Username = chatLogDto.Username,
                ChatType = (ChatType) Enum.Parse(typeof(ChatType), chatLogDto.ChatType),
                Message = chatLogDto.Message,
                Timestamp = DateTime.UtcNow
            };

            _legacyContext.ChatLogs.Add(chatLog);
            _legacyContext.SaveChanges();
        }
    }
}