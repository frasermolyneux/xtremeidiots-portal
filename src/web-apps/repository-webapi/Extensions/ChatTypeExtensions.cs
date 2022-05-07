using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.RepositoryWebApi.Extensions
{
    public static class ChatTypeExtensions
    {
        public static ChatType ToChatType(this int chatType)
        {
            return (ChatType)chatType;
        }

        public static int ToChatTypeInt(this ChatType chatType)
        {
            return (int)chatType;
        }
    }
}
