using System;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Extensions
{
    public static class AdminActionTypeExtensions
    {
        public static AdminActionType ToAdminActionType(this string adminActionType)
        {
            return Enum.Parse<AdminActionType>(adminActionType);
        }
    }
}
