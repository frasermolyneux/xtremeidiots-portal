using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace XI.Portal.Web.Controllers
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