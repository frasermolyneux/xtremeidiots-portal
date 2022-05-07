using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.RepositoryWebApi.Extensions
{
    public static class AdminActionTypeExtensions
    {
        public static AdminActionType ToAdminActionType(this int adminActionType)
        {
            return (AdminActionType)adminActionType;
        }

        public static int ToAdminActionTypeInt(this AdminActionType adminActionType)
        {
            return (int)adminActionType;
        }
    }
}
