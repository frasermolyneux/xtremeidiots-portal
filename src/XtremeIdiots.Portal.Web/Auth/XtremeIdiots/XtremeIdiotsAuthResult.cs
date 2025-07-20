namespace XtremeIdiots.Portal.Web.Auth.XtremeIdiots;

/// <summary>
/// Represents the result of an XtremeIdiots authentication attempt
/// </summary>
public enum XtremeIdiotsAuthResult
{
    /// <summary>
    /// Authentication was successful
    /// </summary>
    Success = 0,

    /// <summary>
    /// User account is locked
    /// </summary>
    Locked = 1,

    /// <summary>
    /// Authentication failed
    /// </summary>
    Failed = 2
}