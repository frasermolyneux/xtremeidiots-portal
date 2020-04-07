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
    public class BanFileMonitorsController : Controller
    {
        private readonly LegacyPortalContext _context;

        public BanFileMonitorsController(LegacyPortalContext context)
        {
            _context = context;
        }

        // GET: BanFileMonitors
        public async Task<IActionResult> Index()
        {
            var legacyPortalContext = _context.BanFileMonitors.Include(b => b.GameServerServer)
                .OrderBy(monitor => monitor.GameServerServer.BannerServerListPosition);
            return View(await legacyPortalContext.ToListAsync());
        }

        // GET: BanFileMonitors/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var banFileMonitors = await _context.BanFileMonitors
                .Include(b => b.GameServerServer)
                .FirstOrDefaultAsync(m => m.BanFileMonitorId == id);
            if (banFileMonitors == null) return NotFound();

            return View(banFileMonitors);
        }

        // GET: BanFileMonitors/Create
        public IActionResult Create()
        {
            ViewData["GameServerServerId"] = new SelectList(_context.GameServers, "ServerId", "ServerId");
            return View();
        }

        // POST: BanFileMonitors/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("BanFileMonitorId,FilePath,RemoteFileSize,LastSync,LastError,GameServerServerId")]
            BanFileMonitors banFileMonitors)
        {
            if (ModelState.IsValid)
            {
                banFileMonitors.BanFileMonitorId = Guid.NewGuid();
                _context.Add(banFileMonitors);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["GameServerServerId"] = new SelectList(_context.GameServers, "ServerId", "ServerId",
                banFileMonitors.GameServerServerId);
            return View(banFileMonitors);
        }

        // GET: BanFileMonitors/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var banFileMonitors = await _context.BanFileMonitors.FindAsync(id);
            if (banFileMonitors == null) return NotFound();
            ViewData["GameServerServerId"] = new SelectList(_context.GameServers, "ServerId", "ServerId",
                banFileMonitors.GameServerServerId);
            return View(banFileMonitors);
        }

        // POST: BanFileMonitors/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id,
            [Bind("BanFileMonitorId,FilePath,RemoteFileSize,LastSync,LastError,GameServerServerId")]
            BanFileMonitors banFileMonitors)
        {
            if (id != banFileMonitors.BanFileMonitorId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(banFileMonitors);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BanFileMonitorsExists(banFileMonitors.BanFileMonitorId))
                        return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["GameServerServerId"] = new SelectList(_context.GameServers, "ServerId", "ServerId",
                banFileMonitors.GameServerServerId);
            return View(banFileMonitors);
        }

        // GET: BanFileMonitors/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var banFileMonitors = await _context.BanFileMonitors
                .Include(b => b.GameServerServer)
                .FirstOrDefaultAsync(m => m.BanFileMonitorId == id);
            if (banFileMonitors == null) return NotFound();

            return View(banFileMonitors);
        }

        // POST: BanFileMonitors/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var banFileMonitors = await _context.BanFileMonitors.FindAsync(id);
            _context.BanFileMonitors.Remove(banFileMonitors);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BanFileMonitorsExists(Guid id)
        {
            return _context.BanFileMonitors.Any(e => e.BanFileMonitorId == id);
        }
    }
}