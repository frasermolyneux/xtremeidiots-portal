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
        public const string ChangeAdminActionAdmin = nameof(ChangeAdminActionAdmin);
        public const string ClaimAdminAction = nameof(ClaimAdminAction);
        public const string CreateAdminAction = nameof(CreateAdminAction);
        public const string CreateAdminActionTopic = nameof(CreateAdminActionTopic);
        public const string DeleteAdminAction = nameof(DeleteAdminAction);
        public const string EditAdminAction = nameof(EditAdminAction);
        public const string LiftAdminAction = nameof(LiftAdminAction);
    }
}