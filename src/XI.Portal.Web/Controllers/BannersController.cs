using Microsoft.AspNetCore.Mvc;

namespace XI.Portal.Web.Controllers
{
    public class BannersController : Controller
    {
        public IActionResult GameServersList()
        {
            return View();
        }
    }
}