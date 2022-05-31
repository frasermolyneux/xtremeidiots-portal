using Microsoft.AspNetCore.Mvc;

namespace XtremeIdiots.Portal.AdminWebApp.Controllers
{
    public class ChangeLogController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}