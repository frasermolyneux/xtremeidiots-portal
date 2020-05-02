using XI.Portal.Data.Legacy.Models;
using XI.Portal.Demos.Dto;

namespace XI.Portal.Demos.Extensions
{
    public static class DemosExtensions
    {
        public static DemoDto ToDto(this Demoes demo)
        {
            var demoDto = new DemoDto
            {
                DemoId = demo.DemoId,
                Game = demo.Game,
                Name = demo.Name,
                FileName = demo.FileName,
                Date = demo.Date,
                Map = demo.Map,
                Mod = demo.Mod,
                GameType = demo.GameType,
                Server = demo.Server,
                Size = demo.Size,

                UserId = demo.User.XtremeIdiotsId,
                UploadedBy = demo.User.UserName
            };

            return demoDto;
        }
    }
}