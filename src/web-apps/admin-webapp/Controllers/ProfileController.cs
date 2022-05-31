using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace XtremeIdiots.Portal.AdminWebApp.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        [HttpGet]
        public IActionResult Manage()
        {
            return View();
        }
    }
}