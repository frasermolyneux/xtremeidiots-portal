using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Demos;

namespace XtremeIdiots.Portal.RepositoryWebApi.Extensions
{
    public static class DemosExtensions
    {
        public static DemoDto ToDto(this Demo demo)
        {
            var dto = new DemoDto
            {
                DemoId = demo.DemoId,
                Game = demo.Game.ToGameType(),
                Name = demo.Name,
                FileName = demo.FileName,
                Date = demo.Date,
                Map = demo.Map,
                Mod = demo.Mod,
                GameType = demo.GameType,
                Server = demo.Server,
                Size = demo.Size,
                DemoFileUri = demo.DemoFileUri
            };

            if (demo.UserProfile != null)
            {
                dto.UserId = demo.UserProfile.XtremeIdiotsForumId;
                dto.UploadedBy = demo.UserProfile.DisplayName;
            }

            return dto;
        }
    }
}
