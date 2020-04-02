using Microsoft.AspNetCore.Mvc;

namespace XI.Portal.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}