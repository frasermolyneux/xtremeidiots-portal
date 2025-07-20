namespace XtremeIdiots.Portal.Web.ViewModels;

/// <summary>
/// View model for pushing maps to remote game servers
/// </summary>
public class PushMapToRemoteViewModel
{
    /// <summary>
    /// Initializes a new instance of the PushMapToRemoteViewModel class
    /// </summary>
    public PushMapToRemoteViewModel()
    {
    }

    /// <summary>
    /// Initializes a new instance of the PushMapToRemoteViewModel class with a game server ID
    /// </summary>
    /// <param name="gameServerId">The ID of the game server</param>
    public PushMapToRemoteViewModel(Guid gameServerId)
    {
        GameServerId = gameServerId;
    }

    /// <summary>
    /// Gets or sets the game server ID
    /// </summary>
    public Guid GameServerId { get; set; }

    /// <summary>
    /// Gets or sets the name of the map to push
    /// </summary>
    public string? MapName { get; set; }
}