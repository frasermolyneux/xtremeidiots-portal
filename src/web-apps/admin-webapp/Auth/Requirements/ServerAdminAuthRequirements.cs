using Microsoft.AspNetCore.Authorization;

namespace XtremeIdiots.Portal.AdminWebApp.Auth.Requirements
{
    public class AccessLiveRcon : IAuthorizationRequirement
    {
    }

    public class AccessServerAdmin : IAuthorizationRequirement
    {
    }

    public class ViewGameChatLog : IAuthorizationRequirement
    {
    }

    public class ViewGlobalChatLog : IAuthorizationRequirement
    {
    }

    public class ViewLiveRcon : IAuthorizationRequirement
    {
    }

    public class ViewServerChatLog : IAuthorizationRequirement
    {
    }

    public class ManageMaps : IAuthorizationRequirement
    {
    }
}
