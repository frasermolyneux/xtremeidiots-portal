using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.AdminWebApp.Auth.Constants;

namespace XtremeIdiots.Portal.AdminWebApp.Controllers
{
    public class ErrorsController : Controller
    {
        public IActionResult Display(int id, [FromServices] IWebHostEnvironment webHostEnvironment)
        {
            if (User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
            {
                var context = HttpContext.Features.Get<IExceptionHandlerFeature>();

                if (context?.Error != null)
                    return Problem(
                        context.Error.StackTrace,
                        title: context.Error.Message);
                else
                    return View(id);
            }

            return View(id);
        }
    }
}