using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XI.Portal.Data.Legacy;
using XI.Portal.Data.Legacy.Models;
using XI.Portal.Web.Constants;
using XI.Portal.Web.Extensions;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = XtremeIdiotsPolicy.Management)]
    public class BanFileMonitorsController : Controller
    {
        private readonly LegacyPortalContext _legacyContext;
        private readonly ILogger<BanFileMonitorsController> _logger;

        public BanFileMonitorsController(LegacyPortalContext legacyContext, ILogger<BanFileMonitorsController> logger)
        {
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var models = _legacyContext.BanFileMonitors
                .ApplyAuthPolicies(User)
                .Include(b => b.GameServerServer)
                .OrderBy(monitor => monitor.GameServerServer.BannerServerListPosition);

            return View(await models.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var model = await _legacyContext.BanFileMonitors
                .ApplyAuthPolicies(User)
                .Include(b => b.GameServerServer)
                .FirstOrDefaultAsync(m => m.BanFileMonitorId == id);

            if (model == null) return NotFound();

            if (!User.HasGameTypeClaim(model.GameServerServer.GameType)) return Unauthorized();

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["GameServerServerId"] = new SelectList(_legacyContext.GameServers
                .ApplyAuthPolicies(User).OrderBy(server => server.BannerServerListPosition), "ServerId", "Title");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("FilePath,GameServerServerId")] BanFileMonitors model)
        {
            if (!User.HasGameTypeClaim(model.GameServerServer.GameType)) return Unauthorized();

            if (ModelState.IsValid)
            {
                model.BanFileMonitorId = Guid.NewGuid();
                model.LastSync = DateTime.UtcNow;

                _legacyContext.Add(model);

                await _legacyContext.SaveChangesAsync();

                _logger.LogInformation(EventIds.Management, "User {User} has created a new ban file monitor with Id {Id}", User.Username(), model.BanFileMonitorId);

                TempData["Success"] = "A new Ban File Monitor has been successfully created";
                return RedirectToAction(nameof(Index));
            }

            ViewData["GameServerServerId"] = new SelectList(
                _legacyContext.GameServers.ApplyAuthPolicies(User).OrderBy(server => server.BannerServerListPosition), "ServerId", "Title",
                model.GameServerServerId);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var model = await _legacyContext.BanFileMonitors.ApplyAuthPolicies(User).FirstOrDefaultAsync(monitor => monitor.BanFileMonitorId == id);

            if (model == null) return NotFound();

            if (!User.HasGameTypeClaim(model.GameServerServer.GameType)) return Unauthorized();

            ViewData["GameServerServerId"] = new SelectList(
                _legacyContext.GameServers.ApplyAuthPolicies(User).OrderBy(server => server.BannerServerListPosition), "ServerId", "Title",
                model.GameServerServerId);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id,
            [Bind("BanFileMonitorId,FilePath,GameServerServerId")]
            BanFileMonitors model)
        {
            if (id != model.BanFileMonitorId) return NotFound();

            if (!User.HasGameTypeClaim(model.GameServerServer.GameType)) return Unauthorized();

            if (ModelState.IsValid)
                try
                {
                    var storedModel = await _legacyContext.BanFileMonitors.ApplyAuthPolicies(User).FirstOrDefaultAsync(monitor => monitor.BanFileMonitorId == id);
                    storedModel.FilePath = model.FilePath;

                    _legacyContext.Update(storedModel);
                    await _legacyContext.SaveChangesAsync();

                    _logger.LogInformation(EventIds.Management, "User {User} has modified a ban file monitor with Id {Id}", User.Username(), id);

                    TempData["Success"] = "The Ban File Monitor has been successfully updated";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BanFileMonitorsExists(model.BanFileMonitorId))
                        return NotFound();
                    throw;
                }

            ViewData["GameServerServerId"] = new SelectList(
                _legacyContext.GameServers.ApplyAuthPolicies(User).OrderBy(server => server.BannerServerListPosition), "ServerId", "Title",
                model.GameServerServerId);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var model = await _legacyContext.BanFileMonitors
                .ApplyAuthPolicies(User)
                .Include(b => b.GameServerServer)
                .FirstOrDefaultAsync(m => m.BanFileMonitorId == id);

            if (model == null) return NotFound();

            if (!User.HasGameTypeClaim(model.GameServerServer.GameType)) return Unauthorized();

            return View(model);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var model = await _legacyContext.BanFileMonitors.ApplyAuthPolicies(User).FirstOrDefaultAsync(monitor => monitor.BanFileMonitorId == id);

            if (!User.HasGameTypeClaim(model.GameServerServer.GameType)) return Unauthorized();

            _legacyContext.BanFileMonitors.Remove(model);
            await _legacyContext.SaveChangesAsync();

            _logger.LogInformation(EventIds.Management, "User {User} has deleted a ban file monitor with Id {Id}", User.Username(), id);

            TempData["Success"] = "The Ban File Monitor has been successfully deleted";
            return RedirectToAction(nameof(Index));
        }

        private bool BanFileMonitorsExists(Guid id)
        {
            return _legacyContext.BanFileMonitors.ApplyAuthPolicies(User).Any(e => e.BanFileMonitorId == id);
        }
    }
}