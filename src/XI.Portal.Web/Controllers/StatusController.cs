using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Servers.Repository;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = XtremeIdiotsPolicy.ViewServiceStatus)]
    public class StatusController : Controller
    {
        private readonly IBanFileMonitorsRepository _banFileMonitorsRepository;

        private readonly string[] _requiredClaims = {XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin, XtremeIdiotsClaimTypes.GameAdmin, XtremeIdiotsClaimTypes.Moderator};

        public StatusController(IBanFileMonitorsRepository banFileMonitorsRepository)
        {
            _banFileMonitorsRepository = banFileMonitorsRepository ?? throw new ArgumentNullException(nameof(banFileMonitorsRepository));
        }

        public async Task<IActionResult> BanFileStatus()
        {
            var statusModel = await _banFileMonitorsRepository.GetStatusModel(User, _requiredClaims);
            return View(statusModel);
        }
    }
}