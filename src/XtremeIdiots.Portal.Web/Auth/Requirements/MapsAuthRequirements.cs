using Microsoft.AspNetCore.Authorization;

namespace XtremeIdiots.Portal.Web.Auth.Requirements;

public class AccessMaps : IAuthorizationRequirement
{
}

public class AccessMapManagerController : IAuthorizationRequirement
{
}

public class ManageMaps : IAuthorizationRequirement
{
}

public class CreateMapPack : IAuthorizationRequirement
{
}

public class EditMapPack : IAuthorizationRequirement
{
}

public class DeleteMapPack : IAuthorizationRequirement
{
}

public class PushMapToRemote : IAuthorizationRequirement
{
}

public class DeleteMapFromHost : IAuthorizationRequirement
{
}