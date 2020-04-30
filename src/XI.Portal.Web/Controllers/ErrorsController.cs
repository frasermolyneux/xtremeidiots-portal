using Microsoft.AspNetCore.Mvc;

ions.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace XI.Portal.Web.Controllers
{
    public class ErrorsController : Controller
  
           public IActionResult Display(int id)
        {
            return View(id);
        }
    }
}
