using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using XI.CommonTypes;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.GameServers.Extensions;
using XI.Portal.Bus.Client;
using XI.Portal.Bus.Models;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = AuthPolicies.AccessMigration)]
    public class ServiceBusController : Controller
    {
        private readonly IPortalServiceBusClient _portalServiceBusClient;

        public ServiceBusController(IPortalServiceBusClient portalServiceBusClient)
        {
            _portalServiceBusClient = portalServiceBusClient;
        }

        [HttpGet]
        public IActionResult MapVote()
        {
            AddGameTypeViewData();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> MapVote(MapVote model)
        {
            await _portalServiceBusClient.PostMapVote(model);
            return RedirectToAction("MapVote");
        }

        private void AddGameTypeViewData(GameType? selected = null)
        {
            if (selected == null)
                selected = GameType.Unknown;

            var gameTypes = User.GetGameTypesForGameServers();
            ViewData["GameType"] = new SelectList(gameTypes, selected);
        }
    }
}