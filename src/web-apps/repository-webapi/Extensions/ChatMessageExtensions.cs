﻿using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.RepositoryWebApi.Extensions
{
    public static class ChatMessageExtensions
    {
        public static ChatMessageSearchEntryDto ToSearchEntryDto(this ChatLog chatLog)
        {
            var dto = new ChatMessageSearchEntryDto
            {
                ChatLogId = chatLog.ChatLogId,
                Timestamp = chatLog.Timestamp,
                Username = chatLog.Username,
                ChatType = chatLog.ChatType.ToChatType(),
                Message = chatLog.Message
            };

            if (chatLog.PlayerPlayerId != null)
                dto.PlayerId = (Guid)chatLog.PlayerPlayerId;

            if (chatLog.GameServerServerId != null)
                dto.ServerId = (Guid)chatLog.GameServerServerId;

            if (chatLog.GameServerServer != null)
            {
                dto.ServerName = chatLog.GameServerServer.Title;
                dto.GameType = chatLog.GameServerServer.GameType.ToGameType();
            }

            return dto;
        }
    }
}