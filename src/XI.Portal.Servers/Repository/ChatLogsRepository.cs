﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using XI.Portal.Data.Legacy;
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

            return await filterModel.ApplyFilter(_legacyContext).CountAsync();
        }

        public async Task<List<ChatLogDto>> GetChatLogs(ChatLogFilterModel filterModel)
        {
            if (filterModel == null) filterModel = new ChatLogFilterModel();

            var chatLogs = await filterModel.ApplyFilter(_legacyContext).ToListAsync();

            var results = new List<ChatLogDto>();

            foreach (var chatLog in chatLogs)
                results.Add(new ChatLogDto
                {
                    ChatLogId = chatLog.ChatLogId,
                    PlayerId = (Guid) chatLog.PlayerPlayerId,
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
                PlayerId = (Guid) chatLog.PlayerPlayerId,
                GameType = chatLog.GameServerServer.GameType.ToString(),
                Timestamp = chatLog.Timestamp,
                Username = chatLog.Username,
                ChatType = chatLog.ChatType.ToString(),
                Message = chatLog.Message
            };
        }
    }
}