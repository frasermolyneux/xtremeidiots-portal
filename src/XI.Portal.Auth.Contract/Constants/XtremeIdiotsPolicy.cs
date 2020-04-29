namespace XI.Portal.Auth.Contract.Constants
{
    public static class XtremeIdiotsPolicy
    {
        public const string RootPolicy = nameof(RootPolicy);
        public const string ServersManagement = nameof(ServersManagement);
        public const string PlayersManagement = nameof(PlayersManagement);
        public const string UserHasCredentials = nameof(UserHasCredentials);
        public const string ViewServiceStatus = nameof(ViewServiceStatus);
        public const string CanAccessLiveRcon = nameof(CanAccessLiveRcon);
        public const string CanAccessServerAdmin = nameof(CanAccessServerAdmin);
        public const string CanAccessGlobalChatLog = nameof(CanAccessGlobalChatLog);
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
        public const string CreateBanFileMonitor = nameof(CreateBanFileMonitor);
        public const string ViewBanFileMonitor = nameof(ViewBanFileMonitor);
        public const string EditBanFileMonitor = nameof(EditBanFileMonitor);
        public const string DeleteBanFileMonitor = nameof(DeleteBanFileMonitor);

        // Game Servers
        public const string CreateGameServer = nameof(CreateGameServer);
        public const string ViewGameServer = nameof(ViewGameServer);
        public const string EditGameServer = nameof(EditGameServer);
        public const string EditGameServerFtp = nameof(EditGameServerFtp);
        public const string EditGameServerRcon = nameof(EditGameServerRcon);
        public const string DeleteGameServer = nameof(DeleteGameServer);
        
    }
}