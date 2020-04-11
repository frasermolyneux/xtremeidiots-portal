using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XI.Portal.Auth.Contract.Constants;

namespace XI.Portal.Web.Controllers
{
    [AllowAnonymous]
    public class PermissionsController : Controller
    {
        [Authorize(Policy = XtremeIdiotsPolicy.SeniorAdmin)]
        public IActionResult SeniorAdmin()
        {
            return Json("OK");
        }

        [Authorize(Policy = XtremeIdiotsPolicy.HeadAdmin)]
        public IActionResult HeadAdmin()
        {
            return Json("OK");
        }

        [Authorize(Policy = XtremeIdiotsPolicy.Admin)]
        public IActionResult Admin()
        {
            return Json("OK");
        }

        [Authorize(Policy = XtremeIdiotsPolicy.HeadAdminX)]
        public IActionResult HeadAdminX()
        {
            return Json("OK");
        }

        [Authorize(Policy = XtremeIdiotsPolicy.AdminX)]
        public IActionResult AdminX()
        {
            return Json("OK");
        }
    }
}