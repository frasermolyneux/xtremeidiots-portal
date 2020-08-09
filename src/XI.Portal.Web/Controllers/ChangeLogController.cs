using Microsoft.AspNetCore.Mvc;

namespace XI.Portal.Web.Controllers
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