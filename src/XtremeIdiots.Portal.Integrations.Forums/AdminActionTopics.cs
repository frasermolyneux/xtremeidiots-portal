using Microsoft.Extensions.Logging;

using System.Globalization;

using XtremeIdiots.InvisionCommunity;
using XtremeIdiots.Portal.Integrations.Forums.Extensions;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;

namespace XtremeIdiots.Portal.Integrations.Forums;

public class AdminActionTopics(ILogger<AdminActionTopics> logger, IInvisionApiClient forumsClient) : IAdminActionTopics
{
    private readonly IInvisionApiClient invisionClient = forumsClient ?? throw new ArgumentNullException(nameof(forumsClient));
    private readonly ILogger<AdminActionTopics> logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<int> CreateTopicForAdminAction(AdminActionType type, GameType gameType, Guid playerId, string username, DateTime created, string text, string? adminId)
    {
        try
        {
            var userId = 21145;

            if (adminId != null)
                userId = Convert.ToInt32(adminId);

            var forumId = 28;
            switch (type)
            {
                case AdminActionType.Observation:
                    forumId = gameType.ForumIdForObservations();
                    break;
                case AdminActionType.Warning:
                    forumId = gameType.ForumIdForWarnings();
                    break;
                case AdminActionType.Kick:
                    forumId = gameType.ForumIdForKicks();
                    break;
                case AdminActionType.TempBan:
                    forumId = gameType.ForumIdForTempBans();
                    break;
                case AdminActionType.Ban:
                    forumId = gameType.ForumIdForBans();
                    break;
                default:
                    break;
            }

            var postTopicResult = await invisionClient.Forums.PostTopic(forumId, userId, $"{username} - {type}", PostContent(type, playerId, username, created, text), type.ToString());

            if (postTopicResult is null)
            {
                logger.LogWarning("Failed to create forum topic for admin action - null response");
                return 0;
            }

            return postTopicResult.TopicId;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating admin action topic");
            return 0;
        }
    }

    public async Task UpdateTopicForAdminAction(int topicId, AdminActionType type, GameType gameType, Guid playerId, string username, DateTime created, string text, string? adminId)
    {
        if (topicId == 0)
            return;

        var userId = 21145;

        if (adminId != null)
            userId = Convert.ToInt32(adminId);

        await invisionClient.Forums.UpdateTopic(topicId, userId, PostContent(type, playerId, username, created, text));
    }

    private static string PostContent(AdminActionType type, Guid playerId, string username, DateTime created, string text)
    {
        return "<p>" +
               $"   Username: {username}<br>" +
               $"   Player Link: <a href=\"https://portal.xtremeidiots.com/Players/Details/{playerId}\">Portal</a><br>" +
               $"   {type} Created: {created.ToString(CultureInfo.InvariantCulture)}" +
               "</p>" +
               "<p>" +
               $"   {text}" +
               "</p>" +
               "<p>" +
               "   <small>Do not edit this post directly as it will be overwritten by the Portal. Add comments on posts below or edit the record in the Portal.</small>" +
               "</p>";
    }
}