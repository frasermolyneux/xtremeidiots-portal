namespace XtremeIdiots.Portal.Web.Auth.XtremeIdiots
{
    /// <summary>
    /// Represents the result of an XtremeIdiots authentication attempt.
    /// </summary>
    public enum XtremeIdiotsAuthResult
    {
        /// <summary>
        /// Authentication was successful.
        /// </summary>
        Success = 0,

        /// <summary>
        /// The user account is locked and cannot authenticate.
        /// </summary>
        Locked = 1,

        /// <summary>
        /// Authentication failed due to invalid credentials or other error.
        /// </summary>
        Failed = 2
    }
}