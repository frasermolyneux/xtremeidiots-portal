using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.RepositoryWebApi.Extensions
{
    public static class AdminActionExtensions
    {
        public static AdminActionDto ToDto(this AdminAction adminAction)
        {
            var dto = new AdminActionDto
            {
                AdminActionId = adminAction.AdminActionId,
                Type = adminAction.Type.ToAdminActionType(),
                Text = adminAction.Text,
                Expires = adminAction.Expires,
                ForumTopicId = adminAction.ForumTopicId,
                Created = adminAction.Created,
            };

            if (adminAction.PlayerPlayer != null)
            {
                dto.PlayerId = adminAction.PlayerPlayer.PlayerId;
                dto.GameType = adminAction.PlayerPlayer.GameType.ToGameType();
                dto.Username = adminAction.PlayerPlayer.Username;
                dto.Guid = adminAction.PlayerPlayer.Guid;
            }

            if (adminAction.Admin != null)
            {
                dto.AdminId = adminAction.Admin.XtremeIdiotsId;
                dto.AdminName = adminAction.Admin.UserName;
            }

            return dto;
        }
    }
}
