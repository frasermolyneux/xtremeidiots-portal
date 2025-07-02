using Microsoft.AspNetCore.Mvc;

namespace XtremeIdiots.Portal.Web.Controllers
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