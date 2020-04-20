using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using XI.CommonTypes;
using XI.Portal.Data.Legacy;
using XI.Portal.Data.Legacy.Models;
using XI.Portal.Servers.Models;

namespace XI.Portal.Servers.Extensions
{
    public static class ChatLogFilterModelExtensions
    {
        public static IQueryable<ChatLogs> ApplyFilter(this ChatLogFilterModel filterModel, LegacyPortalContext legacyContext)
        {
            var chatLogs = legacyContext.ChatLogs.Include(cl => cl.GameServerServer).AsQueryable(); //

            if (filterModel.GameType != GameType.Unknown) chatLogs = chatLogs.Where(m => m.GameServerServer.GameType == filterModel.GameType).AsQueryable();

            if (filterModel.ServerId != Guid.Empty) chatLogs = chatLogs.Where(m => m.GameServerServerId == filterModel.ServerId).AsQueryable();

            if (!string.IsNullOrWhiteSpace(filterModel.FilterString))
                chatLogs = chatLogs.Where(m => m.Message.Contains(filterModel.FilterString)
                                               || m.Username.Contains(filterModel.FilterString)).AsQueryable();

            switch (filterModel.Order)
            {
                case ChatLogFilterModel.OrderBy.TimestampAsc:
                    chatLogs = chatLogs.OrderBy(cl => cl.Timestamp).AsQueryable();
                    break;
                case ChatLogFilterModel.OrderBy.TimestampDesc:
                    chatLogs = chatLogs.OrderByDescending(cl => cl.Timestamp).AsQueryable();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            chatLogs = chatLogs.Skip(filterModel.SkipEntries).AsQueryable();

            if (filterModel.TakeEntries != 0) chatLogs = chatLogs.Take(filterModel.TakeEntries).AsQueryable();

            return chatLogs;
        }
    }
}