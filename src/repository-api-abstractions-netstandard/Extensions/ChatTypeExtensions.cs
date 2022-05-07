using System;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Extensions
{
    public static class ChatTypeExtensions
    {
        public static ChatType ToChatType(this string chatType)
        {
            return Enum.Parse<ChatType>(chatType);
        }
    }
}
