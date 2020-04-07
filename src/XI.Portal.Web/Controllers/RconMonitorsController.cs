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
    public class RconMonitorsController : Controller
    {
        private readonly LegacyPortalContext _legacyContext;
        private readonly ILogger<RconMonitorsController> _logger;

        public RconMonitorsController(LegacyPortalContext legacyContext, ILogger<RconMonitorsController> logger)
        {
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var models = _legacyContext.RconMonitors
                .Include(r => r.GameServerServer)
                .OrderBy(monitor => monitor.GameServerServer.BannerServerListPosition);

            return View(await models.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var model = await _legacyContext.RconMonitors
                .Include(r => r.GameServerServer)
                .FirstOrDefaultAsync(m => m.RconMonitorId == id);

            if (model == null) return NotFound();

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["GameServerServerId"] = new SelectList(_legacyContext.GameServers.OrderBy(server => server.BannerServerListPosition), "ServerId", "Title");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("MonitorMapRotation,MonitorPlayers,MonitorPlayerIps,GameServerServerId")]
            RconMonitors model)
        {
            if (ModelState.IsValid)
            {
                model.RconMonitorId = Guid.NewGuid();
                model.LastUpdated = DateTime.UtcNow;
                model.MapRotationLastUpdated = DateTime.UtcNow;

                _legacyContext.Add(model);

                await _legacyContext.SaveChangesAsync();

                _logger.LogInformation(EventIds.Management, "User {User} has created a new rcon monitor with Id {Id}", User.Username(), model.RconMonitorId);

                TempData["Success"] = "A new Rcon Monitor has been successfully created";
                return RedirectToAction(nameof(Index));
            }

            ViewData["GameServerServerId"] = new SelectList(_legacyContext.GameServers.OrderBy(server => server.BannerServerListPosition), "ServerId", "Title",
                model.GameServerServerId);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var model = await _legacyContext.RconMonitors.FindAsync(id);

            if (model == null) return NotFound();

            ViewData["GameServerServerId"] = new SelectList(_legacyContext.GameServers.OrderBy(server => server.BannerServerListPosition), "ServerId", "Title",
                model.GameServerServerId);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id,
            [Bind("RconMonitorId,MonitorMapRotation,MonitorPlayers,MonitorPlayerIps,GameServerServerId")]
            RconMonitors model)
        {
            if (id != model.RconMonitorId) return NotFound();

            if (ModelState.IsValid)
                try
                {
                    var storedModel = await _legacyContext.RconMonitors.FindAsync(id);
                    storedModel.MonitorMapRotation = model.MonitorMapRotation;
                    storedModel.MonitorPlayers = model.MonitorPlayers;
                    storedModel.MonitorPlayerIps = model.MonitorPlayerIps;

                    _legacyContext.Update(storedModel);
                    await _legacyContext.SaveChangesAsync();

                    _logger.LogInformation(EventIds.Management, "User {User} has modified a rcon monitor with Id {Id}", User.Username(), id);

                    TempData["Success"] = "The Rcon Monitor has been successfully updated";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RconMonitorsExists(model.RconMonitorId))
                        return NotFound();
                    throw;
                }

            ViewData["GameServerServerId"] = new SelectList(_legacyContext.GameServers.OrderBy(server => server.BannerServerListPosition), "ServerId", "Title",
                model.GameServerServerId);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var rconMonitors = await _legacyContext.RconMonitors
                .Include(r => r.GameServerServer)
                .FirstOrDefaultAsync(m => m.RconMonitorId == id);

            if (rconMonitors == null) return NotFound();

            return View(rconMonitors);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var rconMonitors = await _legacyContext.RconMonitors.FindAsync(id);
            _legacyContext.RconMonitors.Remove(rconMonitors);
            await _legacyContext.SaveChangesAsync();

            _logger.LogInformation(EventIds.Management, "User {User} has deleted a rcon monitor with Id {Id}", User.Username(), id);

            TempData["Success"] = "The Rcon Monitor has been successfully deleted";
            return RedirectToAction(nameof(Index));
        }

        private bool RconMonitorsExists(Guid id)
        {
            return _legacyContext.RconMonitors.Any(e => e.RconMonitorId == id);
        }
    }
}