namespace XtremeIdiots.Portal.Web.Auth.Constants
{
    /// <summary>
    /// Contains all authorization policy constants used throughout the XtremeIdiots Portal application.
    /// These constants are used with the [Authorize(Policy = AuthPolicies.PolicyName)] attribute
    /// to enforce granular permissions on controller actions and methods.
    /// </summary>
    /// <remarks>
    /// Policy names are generated using nameof() to ensure compile-time safety and proper refactoring support.
    /// Each policy corresponds to a specific authorization requirement defined in the application's
    /// authorization handlers and policy configuration.
    /// </remarks>
    public static class AuthPolicies
    {
        // Admin Actions
        public const string AccessAdminActionsController = nameof(AccessAdminActionsController);
        public const string ChangeAdminActionAdmin = nameof(ChangeAdminActionAdmin);
        public const string ClaimAdminAction = nameof(ClaimAdminAction);
        public const string CreateAdminAction = nameof(CreateAdminAction);
        public const string CreateAdminActionTopic = nameof(CreateAdminActionTopic);
        public const string DeleteAdminAction = nameof(DeleteAdminAction);
        public const string EditAdminAction = nameof(EditAdminAction);
        public const string LiftAdminAction = nameof(LiftAdminAction);

        // Ban File Monitors
        public const string AccessBanFileMonitors = nameof(AccessBanFileMonitors);
        public const string CreateBanFileMonitor = nameof(CreateBanFileMonitor);
        public const string ViewBanFileMonitor = nameof(ViewBanFileMonitor);
        public const string EditBanFileMonitor = nameof(EditBanFileMonitor);
        public const string DeleteBanFileMonitor = nameof(DeleteBanFileMonitor);

        // Change Log
        public const string AccessChangeLog = nameof(AccessChangeLog);

        // Credentials
        public const string AccessCredentials = nameof(AccessCredentials);

        // Demos
        public const string AccessDemos = nameof(AccessDemos);
        public const string DeleteDemo = nameof(DeleteDemo);

        // Game Servers
        public const string AccessGameServers = nameof(AccessGameServers);
        public const string CreateGameServer = nameof(CreateGameServer);
        public const string DeleteGameServer = nameof(DeleteGameServer);
        public const string EditGameServerFtp = nameof(EditGameServerFtp);
        public const string EditGameServer = nameof(EditGameServer);
        public const string EditGameServerRcon = nameof(EditGameServerRcon);
        public const string ViewFtpCredential = nameof(ViewFtpCredential);
        public const string ViewGameServer = nameof(ViewGameServer);
        public const string ViewRconCredential = nameof(ViewRconCredential);

        // Home
        public const string AccessHome = nameof(AccessHome);

        // Profile
        public const string AccessProfile = nameof(AccessProfile);

        // Maps
        public const string AccessMaps = nameof(AccessMaps);
        public const string AccessMapManagerController = nameof(AccessMapManagerController);
        public const string ManageMaps = nameof(ManageMaps);
        public const string CreateMapPack = nameof(CreateMapPack);
        public const string EditMapPack = nameof(EditMapPack);
        public const string DeleteMapPack = nameof(DeleteMapPack);
        public const string PushMapToRemote = nameof(PushMapToRemote);
        public const string DeleteMapFromHost = nameof(DeleteMapFromHost);

        // Migration
        public const string AccessMigration = nameof(AccessMigration);

        // Player Tags
        public const string AccessPlayerTags = nameof(AccessPlayerTags);
        public const string CreatePlayerTag = nameof(CreatePlayerTag);
        public const string DeletePlayerTag = nameof(DeletePlayerTag);
        public const string EditPlayerTag = nameof(EditPlayerTag);

        // Players
        public const string AccessPlayers = nameof(AccessPlayers);
        public const string DeletePlayer = nameof(DeletePlayer);
        public const string ViewPlayers = nameof(ViewPlayers);
        public const string CreateProtectedName = nameof(CreateProtectedName);
        public const string DeleteProtectedName = nameof(DeleteProtectedName);
        public const string ViewProtectedName = nameof(ViewProtectedName);

        // Server Admin
        public const string AccessLiveRcon = nameof(AccessLiveRcon);
        public const string AccessServerAdmin = nameof(AccessServerAdmin);
        public const string ViewGameChatLog = nameof(ViewGameChatLog);
        public const string ViewGlobalChatLog = nameof(ViewGlobalChatLog);
        public const string ViewLiveRcon = nameof(ViewLiveRcon);
        public const string ViewServerChatLog = nameof(ViewServerChatLog);
        public const string LockChatMessages = nameof(LockChatMessages);

        // Servers
        public const string AccessServers = nameof(AccessServers);

        // Status
        public const string AccessStatus = nameof(AccessStatus);

        // Users
        public const string AccessUsers = nameof(AccessUsers);
        public const string CreateUserClaim = nameof(CreateUserClaim);
        public const string DeleteUserClaim = nameof(DeleteUserClaim);
    }
}