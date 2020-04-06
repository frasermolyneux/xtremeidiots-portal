using System.Linq;
using ElCamino.AspNetCore.Identity.AzureTable.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XI.Portal.Web.Constants;
using XI.Portal.Web.Data;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = XtremeIdiotsPolicy.SeniorAdmin)]
    public class UserController : Controller
    {
        private readonly ApplicationAuthDbContext _context;


        public UserController(ApplicationAuthDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var query = from user in _context.UserTable.CreateQuery<IdentityUser>()
                where user.Email != ""
                select user;
            var users = query.ToList();

            return View(users);
        }
    }
}