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
    public class FileMonitorsController : Controller
    {
        private readonly LegacyPortalContext _legacyContext;
        private readonly ILogger<FileMonitorsController> _logger;

        public FileMonitorsController(LegacyPortalContext legacyContext, ILogger<FileMonitorsController> logger)
        {
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var models = _legacyContext.FileMonitors
                .ApplyAuthPolicies(User)
                .Include(f => f.GameServerServer)
                .OrderBy(monitor => monitor.GameServerServer.BannerServerListPosition);
            ;

            return View(await models.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var model = await _legacyContext.FileMonitors
                .ApplyAuthPolicies(User)
                .Include(f => f.GameServerServer)
                .FirstOrDefaultAsync(m => m.FileMonitorId == id);

            if (model == null) return NotFound();

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
            [Bind("FilePath,GameServerServerId")] FileMonitors model)
        {
            if (!User.HasGameTypeClaim(model.GameServerServer.GameType)) return Unauthorized();

            if (ModelState.IsValid)
            {
                model.FileMonitorId = Guid.NewGuid();
                model.LastRead = DateTime.UtcNow;

                _legacyContext.Add(model);

                await _legacyContext.SaveChangesAsync();

                _logger.LogInformation(EventIds.Management, "User {User} has created a new file monitor with Id {Id}", User.Username(), model.FileMonitorId);

                TempData["Success"] = "A new File Monitor has been successfully created";
                return RedirectToAction(nameof(Index));
            }

            ViewData["GameServerServerId"] = new SelectList(_legacyContext.GameServers
                    .ApplyAuthPolicies(User).OrderBy(server => server.BannerServerListPosition), "ServerId", "Title",
                model.GameServerServerId);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var model = await _legacyContext.FileMonitors.ApplyAuthPolicies(User).FirstOrDefaultAsync(monitor => monitor.FileMonitorId == id);

            if (model == null) return NotFound();

            ViewData["GameServerServerId"] = new SelectList(_legacyContext.GameServers
                    .ApplyAuthPolicies(User).OrderBy(server => server.BannerServerListPosition), "ServerId", "Title",
                model.GameServerServerId);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id,
            [Bind("FileMonitorId,FilePath,GameServerServerId")]
            FileMonitors model)
        {
            if (id != model.FileMonitorId) return NotFound();

            if (ModelState.IsValid)
                try
                {
                    var storedModel = await _legacyContext.FileMonitors.ApplyAuthPolicies(User).FirstOrDefaultAsync(monitor => monitor.FileMonitorId == id);
                    storedModel.FilePath = model.FilePath;

                    _legacyContext.Update(storedModel);
                    await _legacyContext.SaveChangesAsync();

                    _logger.LogInformation(EventIds.Management, "User {User} has modified a file monitor with Id {Id}", User.Username(), id);

                    TempData["Success"] = "The File Monitor has been successfully updated";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FileMonitorsExists(model.FileMonitorId))
                        return NotFound();
                    throw;
                }

            ViewData["GameServerServerId"] = new SelectList(_legacyContext.GameServers
                    .ApplyAuthPolicies(User).OrderBy(server => server.BannerServerListPosition), "ServerId", "Title",
                model.GameServerServerId);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var model = await _legacyContext.FileMonitors
                .ApplyAuthPolicies(User)
                .Include(f => f.GameServerServer)
                .FirstOrDefaultAsync(m => m.FileMonitorId == id);

            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var model = await _legacyContext.FileMonitors.ApplyAuthPolicies(User).FirstOrDefaultAsync(monitor => monitor.FileMonitorId == id);

            _legacyContext.FileMonitors.Remove(model);
            await _legacyContext.SaveChangesAsync();

            _logger.LogInformation(EventIds.Management, "User {User} has deleted a file monitor with Id {Id}", User.Username(), id);

            TempData["Success"] = "The File Monitor has been successfully deleted";
            return RedirectToAction(nameof(Index));
        }

        private bool FileMonitorsExists(Guid id)
        {
            return _legacyContext.FileMonitors.ApplyAuthPolicies(User).Any(e => e.FileMonitorId == id);
        }
    }
}