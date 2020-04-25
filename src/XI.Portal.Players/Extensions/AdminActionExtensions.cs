using XI.Portal.Data.Legacy.Models;
using XI.Portal.Players.Dto;

namespace XI.Portal.Players.Extensions
{
    public static class AdminActionExtensions
    {
        public static AdminActionDto ToDto(this AdminActions adminAction)
        {
            var adminActionDto = new AdminActionDto
            {
                AdminActionId = adminAction.AdminActionId,
                PlayerId = adminAction.PlayerPlayer.PlayerId,
                GameType = adminAction.PlayerPlayer.GameType,
                Username = adminAction.PlayerPlayer.Username,
                Guid = adminAction.PlayerPlayer.Guid,
                Type = adminAction.Type,
                Text = adminAction.Text,
                Expires = adminAction.Expires,
                ForumTopicId = adminAction.ForumTopicId,
                Created = adminAction.Created,
                AdminId = adminAction.Admin?.XtremeIdiotsId,
                AdminName = adminAction.Admin?.UserName
            };

            return adminActionDto;
        }
    }
}