using System;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Extensions
{
    public static class AdminActionTypeExtensions
    {
        public static AdminActionType ToAdminActionType(this string adminActionType)
        {
            return Enum.Parse<AdminActionType>(adminActionType);
        }
    }
}
