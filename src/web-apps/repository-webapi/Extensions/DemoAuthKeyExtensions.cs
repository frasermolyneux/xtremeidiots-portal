using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Demos;

namespace XtremeIdiots.Portal.RepositoryWebApi.Extensions
{
    public static class DemoAuthKeyExtensions
    {
        public static DemoAuthDto ToDto(this DemoAuthKey demoAuthKey)
        {
            var dto = new DemoAuthDto
            {
                UserId = demoAuthKey.UserId,
                AuthKey = demoAuthKey.AuthKey,
                Created = demoAuthKey.Created,
                LastActivity = demoAuthKey.LastActivity
            };

            return dto;
        }
    }
}
