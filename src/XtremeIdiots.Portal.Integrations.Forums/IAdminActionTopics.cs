using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;

namespace XtremeIdiots.Portal.Integrations.Forums;

/// <summary>
/// Service for managing forum topics related to admin actions
/// </summary>
public interface IAdminActionTopics
{
    /// <summary>
    /// Creates a new forum topic for an admin action
    /// </summary>
    /// <param name="type">The type of admin action</param>
    /// <param name="gameType">The game type the action was taken on</param>
    /// <param name="playerId">The unique identifier of the player</param>
    /// <param name="username">The username of the player</param>
    /// <param name="created">The date and time the action was created</param>
    /// <param name="text">The action description or reason</param>
    /// <param name="adminId">The optional identifier of the admin who took the action</param>
    /// <returns>The ID of the created forum topic</returns>
    Task<int> CreateTopicForAdminAction(AdminActionType type, GameType gameType, Guid playerId, string username, DateTime created, string text, string? adminId);

    /// <summary>
    /// Updates an existing forum topic for an admin action
    /// </summary>
    /// <param name="topicId">The ID of the forum topic to update</param>
    /// <param name="type">The type of admin action</param>
    /// <param name="gameType">The game type the action was taken on</param>
    /// <param name="playerId">The unique identifier of the player</param>
    /// <param name="username">The username of the player</param>
    /// <param name="created">The date and time the action was created</param>
    /// <param name="text">The action description or reason</param>
    /// <param name="adminId">The optional identifier of the admin who took the action</param>
    /// <returns>A task representing the asynchronous update operation</returns>
    Task UpdateTopicForAdminAction(int topicId, AdminActionType type, GameType gameType, Guid playerId, string username, DateTime created, string text, string? adminId);
}