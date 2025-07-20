using Microsoft.AspNetCore.Authorization;

namespace XtremeIdiots.Portal.Web.Auth.Requirements;

/// <summary>
/// Authorization requirement for accessing admin actions
/// </summary>
public class AccessAdminActions : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for changing the admin associated with an admin action
/// </summary>
public class ChangeAdminActionAdmin : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for claiming an admin action
/// </summary>
public class ClaimAdminAction : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for creating new admin actions
/// </summary>
public class CreateAdminAction : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for creating forum topics for admin actions
/// </summary>
public class CreateAdminActionTopic : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for deleting admin actions
/// </summary>
public class DeleteAdminAction : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for editing existing admin actions
/// </summary>
public class EditAdminAction : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for lifting (removing) admin actions
/// </summary>
public class LiftAdminAction : IAuthorizationRequirement
{
}