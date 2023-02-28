using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XtremeIdiots.Portal.AdminWebApp.Auth.Constants;

namespace XtremeIdiots.Portal.AdminWebApp.Controllers
{
    [Authorize(Policy = AuthPolicies.AccessHome)]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}