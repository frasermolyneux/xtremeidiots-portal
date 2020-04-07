using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using XI.Portal.Data.Legacy;
using XI.Portal.Data.Legacy.Models;
using XI.Portal.Web.Constants;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = XtremeIdiotsPolicy.Management)]
    public class RconMonitorsController : Controller
    {
        private readonly LegacyPortalContext _context;

        public RconMonitorsController(LegacyPortalContext context)
        {
            _context = context;
        }

        // GET: RconMonitors
        public async Task<IActionResult> Index()
        {
            var legacyPortalContext = _context.RconMonitors.Include(r => r.GameServerServer)
                .OrderBy(monitor => monitor.GameServerServer.BannerServerListPosition); ;
            return View(await legacyPortalContext.ToListAsync());
        }

        // GET: RconMonitors/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var rconMonitors = await _context.RconMonitors
                .Include(r => r.GameServerServer)
                .FirstOrDefaultAsync(m => m.RconMonitorId == id);
            if (rconMonitors == null) return NotFound();

            return View(rconMonitors);
        }

        // GET: RconMonitors/Create
        public IActionResult Create()
        {
            ViewData["GameServerServerId"] = new SelectList(_context.GameServers, "ServerId", "ServerId");
            return View();
        }

        // POST: RconMonitors/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind(
                "RconMonitorId,LastUpdated,MonitorMapRotation,MapRotationLastUpdated,MonitorPlayers,MonitorPlayerIps,LastError,GameServerServerId")]
            RconMonitors rconMonitors)
        {
            if (ModelState.IsValid)
            {
                rconMonitors.RconMonitorId = Guid.NewGuid();
                _context.Add(rconMonitors);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["GameServerServerId"] = new SelectList(_context.GameServers, "ServerId", "ServerId",
                rconMonitors.GameServerServerId);
            return View(rconMonitors);
        }

        // GET: RconMonitors/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var rconMonitors = await _context.RconMonitors.FindAsync(id);
            if (rconMonitors == null) return NotFound();
            ViewData["GameServerServerId"] = new SelectList(_context.GameServers, "ServerId", "ServerId",
                rconMonitors.GameServerServerId);
            return View(rconMonitors);
        }

        // POST: RconMonitors/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id,
            [Bind(
                "RconMonitorId,LastUpdated,MonitorMapRotation,MapRotationLastUpdated,MonitorPlayers,MonitorPlayerIps,LastError,GameServerServerId")]
            RconMonitors rconMonitors)
        {
            if (id != rconMonitors.RconMonitorId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rconMonitors);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RconMonitorsExists(rconMonitors.RconMonitorId))
                        return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["GameServerServerId"] = new SelectList(_context.GameServers, "ServerId", "ServerId",
                rconMonitors.GameServerServerId);
            return View(rconMonitors);
        }

        // GET: RconMonitors/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var rconMonitors = await _context.RconMonitors
                .Include(r => r.GameServerServer)
                .FirstOrDefaultAsync(m => m.RconMonitorId == id);
            if (rconMonitors == null) return NotFound();

            return View(rconMonitors);
        }

        // POST: RconMonitors/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var rconMonitors = await _context.RconMonitors.FindAsync(id);
            _context.RconMonitors.Remove(rconMonitors);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RconMonitorsExists(Guid id)
        {
            return _context.RconMonitors.Any(e => e.RconMonitorId == id);
        }
    }
}