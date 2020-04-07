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
    public class FileMonitorsController : Controller
    {
        private readonly LegacyPortalContext _context;

        public FileMonitorsController(LegacyPortalContext context)
        {
            _context = context;
        }

        // GET: FileMonitors
        public async Task<IActionResult> Index()
        {
            var legacyPortalContext = _context.FileMonitors.Include(f => f.GameServerServer)
                .OrderBy(monitor => monitor.GameServerServer.BannerServerListPosition); ;
            return View(await legacyPortalContext.ToListAsync());
        }

        // GET: FileMonitors/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var fileMonitors = await _context.FileMonitors
                .Include(f => f.GameServerServer)
                .FirstOrDefaultAsync(m => m.FileMonitorId == id);
            if (fileMonitors == null) return NotFound();

            return View(fileMonitors);
        }

        // GET: FileMonitors/Create
        public IActionResult Create()
        {
            ViewData["GameServerServerId"] = new SelectList(_context.GameServers, "ServerId", "ServerId");
            return View();
        }

        // POST: FileMonitors/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("FileMonitorId,FilePath,BytesRead,LastRead,LastError,GameServerServerId")]
            FileMonitors fileMonitors)
        {
            if (ModelState.IsValid)
            {
                fileMonitors.FileMonitorId = Guid.NewGuid();
                _context.Add(fileMonitors);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["GameServerServerId"] = new SelectList(_context.GameServers, "ServerId", "ServerId",
                fileMonitors.GameServerServerId);
            return View(fileMonitors);
        }

        // GET: FileMonitors/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var fileMonitors = await _context.FileMonitors.FindAsync(id);
            if (fileMonitors == null) return NotFound();
            ViewData["GameServerServerId"] = new SelectList(_context.GameServers, "ServerId", "ServerId",
                fileMonitors.GameServerServerId);
            return View(fileMonitors);
        }

        // POST: FileMonitors/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id,
            [Bind("FileMonitorId,FilePath,BytesRead,LastRead,LastError,GameServerServerId")]
            FileMonitors fileMonitors)
        {
            if (id != fileMonitors.FileMonitorId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(fileMonitors);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FileMonitorsExists(fileMonitors.FileMonitorId))
                        return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["GameServerServerId"] = new SelectList(_context.GameServers, "ServerId", "ServerId",
                fileMonitors.GameServerServerId);
            return View(fileMonitors);
        }

        // GET: FileMonitors/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var fileMonitors = await _context.FileMonitors
                .Include(f => f.GameServerServer)
                .FirstOrDefaultAsync(m => m.FileMonitorId == id);
            if (fileMonitors == null) return NotFound();

            return View(fileMonitors);
        }

        // POST: FileMonitors/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var fileMonitors = await _context.FileMonitors.FindAsync(id);
            _context.FileMonitors.Remove(fileMonitors);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FileMonitorsExists(Guid id)
        {
            return _context.FileMonitors.Any(e => e.FileMonitorId == id);
        }
    }
}