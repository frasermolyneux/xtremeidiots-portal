using Microsoft.Extensions.Logging;

using System.Globalization;

using XtremeIdiots.Portal.ForumsIntegration.Extensions;
using XtremeIdiots.Portal.InvisionApiClient;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.ForumsIntegration
{
    public class AdminActionTopics : IAdminActionTopics
    {
        private readonly IInvisionApiClient _invisionClient;
        private readonly ILogger<AdminActionTopics> _logger;

        public AdminActionTopics(ILogger<AdminActionTopics> logger, IInvisionApiClient forumsClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _invisionClient = forumsClient ?? throw new ArgumentNullException(nameof(forumsClient));
        }

        public async Task<int> CreateTopicForAdminAction(AdminActionType type, GameType gameType, Guid playerId, string username, DateTime created, string text, string? adminId)
        {
            try
            {
                var userId = 21145; // Admin

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
                }
                var postTopicResult = await _invisionClient.Forums.PostTopic(forumId, userId, $"{username} - {type}", PostContent(type, playerId, username, created, text), type.ToString());
                return postTopicResult.TopicId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating admin action topic");
                return 0;
            }
        }

        public async Task UpdateTopicForAdminAction(int topicId, AdminActionType type, GameType gameType, Guid playerId, string username, DateTime created, string text, string? adminId)
        {
            if (topicId == 0)
                return;

            var userId = 21145; // Admin

            if (adminId != null)
                userId = Convert.ToInt32(adminId);

            await _invisionClient.Forums.UpdateTopic(topicId, userId, PostContent(type, playerId, username, created, text));
        }

        private string PostContent(AdminActionType type, Guid playerId, string username, DateTime created, string text)
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
}
