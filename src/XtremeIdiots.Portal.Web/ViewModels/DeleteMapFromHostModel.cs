namespace XtremeIdiots.Portal.Web.ViewModels;

/// <summary>
/// View model for deleting a map from a game server host
/// </summary>
public class DeleteMapFromHostModel
{
    /// <summary>
    /// Initializes a new instance of the DeleteMapFromHostModel class
    /// </summary>
    public DeleteMapFromHostModel()
    {
    }

    /// <summary>
    /// Initializes a new instance of the DeleteMapFromHostModel class with server ID and map name
    /// </summary>
    /// <param name="gameServerId">The unique identifier of the game server</param>
    /// <param name="mapName">The name of the map to delete</param>
    public DeleteMapFromHostModel(Guid gameServerId, string mapName)
    {
        GameServerId = gameServerId;
        MapName = mapName;
    }

    /// <summary>
    /// Gets or sets the unique identifier of the game server
    /// </summary>
    public Guid GameServerId { get; set; }

    /// <summary>
    /// Gets or sets the name of the map to delete
    /// </summary>
    public string? MapName { get; set; }
}