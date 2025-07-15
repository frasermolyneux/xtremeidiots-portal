using Microsoft.AspNetCore.Authorization;

namespace XtremeIdiots.Portal.Web.Auth.Requirements
{
    /// <summary>
    /// Authorization requirement for accessing the players functionality.
    /// Allows users to view and browse player information but not perform
    /// management operations. This is the base permission for player-related features.
    /// </summary>
    public class AccessPlayers : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for deleting player records.
    /// Allows permanent removal of player data from the system.
    /// This is a high-privilege operation typically restricted to senior administrators.
    /// </summary>
    public class DeletePlayer : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for viewing detailed player information.
    /// Allows users to access player profiles, statistics, and historical data
    /// for Call of Duty players across different game types.
    /// Authorization is scoped to specific game types.
    /// </summary>
    public class ViewPlayers : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for creating protected player names.
    /// Allows users to add names to the protected list, preventing other players
    /// from using reserved or inappropriate names on Call of Duty servers.
    /// </summary>
    public class CreateProtectedName : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for deleting protected player names.
    /// Allows removal of names from the protected names list.
    /// This enables management of the reserved name system.
    /// </summary>
    public class DeleteProtectedName : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for viewing the protected names list.
    /// Allows users to see which player names are protected and reserved
    /// in the system without modification capabilities.
    /// </summary>
    public class ViewProtectedName : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for accessing the player tags functionality.
    /// Allows users to view and browse player tags but not manage them.
    /// Player tags are used to categorize and organize players.
    /// </summary>
    public class AccessPlayerTags : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for creating new player tags.
    /// Allows users to add tags to players for categorization purposes,
    /// such as marking VIP players, troublemakers, or other classifications.
    /// </summary>
    public class CreatePlayerTag : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for editing existing player tags.
    /// Allows modification of player tag properties, descriptions,
    /// and associated player assignments.
    /// </summary>
    public class EditPlayerTag : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for deleting player tags.
    /// Allows removal of tags from players and permanent deletion
    /// of tag categories from the system.
    /// </summary>
    public class DeletePlayerTag : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for viewing player tag information.
    /// Allows users to see what tags are assigned to players
    /// and understand player categorizations.
    /// </summary>
    public class ViewPlayerTag : IAuthorizationRequirement
    {
    }
}
