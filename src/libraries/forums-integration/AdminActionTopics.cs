using Microsoft.Extensions.Logging;
using System.Globalization;
using XtremeIdiots.Portal.ForumsIntegration.Extensions;
using XtremeIdiots.Portal.InvisionApiClient;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.AdminActions;

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

        public async Task<int> CreateTopicForAdminAction(AdminActionDto model)
        {
            try
            {
                var userId = 21145; // Admin
                if (model.AdminId != null)
                    userId = Convert.ToInt32(model.AdminId);

                var forumId = 28;
                switch (model.Type)
                {
                    case AdminActionType.Observation:
                        forumId = model.GameType.ForumIdForObservations();
                        break;
                    case AdminActionType.Warning:
                        forumId = model.GameType.ForumIdForWarnings();
                        break;
                    case AdminActionType.Kick:
                        forumId = model.GameType.ForumIdForKicks();
                        break;
                    case AdminActionType.TempBan:
                        forumId = model.GameType.ForumIdForTempBans();
                        break;
                    case AdminActionType.Ban:
                        forumId = model.GameType.ForumIdForBans();
                        break;
                }

                var postTopicResult = await _invisionClient.Forums.PostTopic(forumId, userId, $"{model.Username} - {model.Type}", PostContent(model), model.Type.ToString());
                return postTopicResult.TopicId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating admin action topic");
                return 0;
            }
        }

        public async Task UpdateTopicForAdminAction(AdminActionDto model)
        {
            if (model.ForumTopicId == 0)
                return;

            var userId = 21145; // Admin
            if (model.AdminId != null)
                userId = Convert.ToInt32(model.AdminId);

            await _invisionClient.Forums.UpdateTopic(model.ForumTopicId, userId, PostContent(model));
        }

        private string PostContent(AdminActionDto model)
        {
            return "<p>" +
                   $"   Username: {model.Username}<br>" +
                   $"   Player Link: <a href=\"https://portal.xtremeidiots.com/Players/Details/{model.PlayerId}\">Portal</a><br>" +
                   $"   {model.Type} Created: {model.Created.ToString(CultureInfo.InvariantCulture)}" +
                   "</p>" +
                   "<p>" +
                   $"   {model.Text}" +
                   "</p>" +
                   "<p>" +
                   "   <small>Do not edit this post directly as it will be overwritten by the Portal. Add comments on posts below or edit the record in the Portal.</small>" +
                   "</p>";
        }
    }
}
