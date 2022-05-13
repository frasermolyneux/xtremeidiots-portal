using Microsoft.AspNetCore.Authorization;

namespace XI.Portal.Web.Auth.Requirements
{
    public class AccessGameServers : IAuthorizationRequirement
    {
    }

    public class CreateGameServer : IAuthorizationRequirement
    {
    }

    public class DeleteGameServer : IAuthorizationRequirement
    {
    }

    public class EditGameServer : IAuthorizationRequirement
    {
    }

    public class EditGameServerFtp : IAuthorizationRequirement
    {
    }

    public class EditGameServerRcon : IAuthorizationRequirement
    {
    }

    public class ViewFtpCredential : IAuthorizationRequirement
    {
    }

    public class ViewGameServer : IAuthorizationRequirement
    {
    }

    public class ViewRconCredential : IAuthorizationRequirement
    {
    }
}
