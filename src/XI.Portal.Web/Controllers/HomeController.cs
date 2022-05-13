using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XI.Portal.Web.Auth.Constants;

namespace XI.Portal.Web.Controllers
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