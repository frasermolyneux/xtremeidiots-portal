using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using XI.Portal.Data.Legacy;
using XI.Portal.Demos.Dto;
using XI.Portal.Demos.Extensions;
using XI.Portal.Demos.Interfaces;
using XI.Portal.Demos.Models;

namespace XI.Portal.Demos.Repository
{
    public class DemosRepository : IDemosRepository
    {
        private readonly LegacyPortalContext _legacyContext;
        private readonly IDemosRepositoryOptions _options;

        public DemosRepository(IDemosRepositoryOptions options, LegacyPortalContext legacyContext)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));
        }

        public async Task<List<DemoDto>> GetDemos(DemosFilterModel filterModel, ClaimsPrincipal user, string[] requiredClaims)
        {
            var demos = await _legacyContext.Demoes.ApplyAuthPolicies(user, requiredClaims).ApplyFilter(filterModel).ToListAsync();

            var results = new List<DemoDto>();

            foreach (var demo in demos)
            {
                var demoDto = new DemoDto
                {
                    DemoId = demo.DemoId,
                    Game = demo.Game.ToString(),
                    Name = demo.Name,
                    FileName = demo.FileName,
                    Date = demo.Date,
                    Map = demo.Map,
                    Mod = demo.Mod,
                    GameType = demo.GameType,
                    Server = demo.Server,
                    Size = demo.Size,

                    UserId = demo.UserId,
                    UploadedBy = demo.User.UserName
                };

                results.Add(demoDto);
            }

            return results;
        }

        public async Task<int> GetDemoCount(DemosFilterModel filterModel, ClaimsPrincipal user, string[] requiredClaims)
        {
            return await _legacyContext.Demoes.ApplyAuthPolicies(user, requiredClaims).ApplyFilter(filterModel).CountAsync();
        }
    }
}