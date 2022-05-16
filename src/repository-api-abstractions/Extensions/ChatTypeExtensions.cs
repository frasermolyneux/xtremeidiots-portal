using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Extensions
{
    public static class ChatTypeExtensions
    {
        public static ChatType ToChatType(this string chatType)
        {
            return Enum.Parse<ChatType>(chatType);
        }
    }
}
