using Microsoft.AspNetCore.Authorization;

namespace XtremeIdiots.Portal.Web.Auth.Requirements;

/// <summary>
/// Authorization requirement for accessing ban file monitor functionality
/// </summary>
public class AccessBanFileMonitors : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for viewing ban file monitor details
/// </summary>
public class ViewBanFileMonitor : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for creating new ban file monitors
/// </summary>
public class CreateBanFileMonitor : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for editing existing ban file monitors
/// </summary>
public class EditBanFileMonitor : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for deleting ban file monitors
/// </summary>
public class DeleteBanFileMonitor : IAuthorizationRequirement
{
}