using Microsoft.AspNetCore.Authorization;

namespace XtremeIdiots.Portal.Web.Auth.Requirements
{

    public class AccessBanFileMonitors : IAuthorizationRequirement
    {
    }

    public class ViewBanFileMonitor : IAuthorizationRequirement
    {
    }

    public class CreateBanFileMonitor : IAuthorizationRequirement
    {
    }

    public class EditBanFileMonitor : IAuthorizationRequirement
    {
    }

    public class DeleteBanFileMonitor : IAuthorizationRequirement
    {
    }
}