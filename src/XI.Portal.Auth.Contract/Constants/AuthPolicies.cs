namespace XI.Portal.Auth.Contract.Constants
{
    public static class AuthPolicies
    {
        public const string RootPolicy = nameof(RootPolicy);
        public const string ServersManagement = nameof(ServersManagement);
        public const string PlayersManagement = nameof(PlayersManagement);
        public const string UserHasCredentials = nameof(UserHasCredentials);
        public const string ViewServiceStatus = nameof(ViewServiceStatus);
        public const string AccessLiveRcon = nameof(AccessLiveRcon);
        public const string AccessServerAdmin = nameof(AccessServerAdmin);
        public const string AccessGlobalChatLog = nameof(AccessGlobalChatLog);
        public const string CanAccessGameChatLog = nameof(CanAccessGameChatLog);

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

        // Credentials
        public const string AccessCredentials = nameof(AccessCredentials);

        // Demos
        public const string AccessDemos = nameof(AccessDemos);
        public const string DeleteDemo = nameof(DeleteDemo);

        // File Monitors
        public const string AccessFileMonitors = nameof(AccessFileMonitors);
        public const string CreateFileMonitor = nameof(CreateFileMonitor);
        public const string ViewFileMonitor = nameof(ViewFileMonitor);
        public const string EditFileMonitor = nameof(EditFileMonitor);
        public const string DeleteFileMonitor = nameof(DeleteFileMonitor);

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

        // Maps
        public const string AccessMaps = nameof(AccessMaps);

        // Maps
        public const string AccessPlayers = nameof(AccessPlayers);

        // Rcon Monitors
        public const string AccessRconMonitors = nameof(AccessRconMonitors);
        public const string CreateRconMonitor = nameof(CreateRconMonitor);
        public const string ViewRconMonitor = nameof(ViewRconMonitor);
        public const string EditRconMonitor = nameof(EditRconMonitor);
        public const string DeleteRconMonitor = nameof(DeleteRconMonitor);

        // Servers
        public const string AccessServers = nameof(AccessServers);
    }
}