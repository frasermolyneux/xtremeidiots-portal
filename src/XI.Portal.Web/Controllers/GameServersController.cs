using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XI.Portal.Data.Legacy;
using XI.Portal.Data.Legacy.Models;
using XI.Portal.Web.Constants;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = XtremeIdiotsPolicy.Management)]
    public class GameServersController : Controller
    {
        private readonly LegacyPortalContext _context;

        public GameServersController(LegacyPortalContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.GameServers.OrderBy(server => server.BannerServerListPosition).ToListAsync());
        }

        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var gameServers = await _context.GameServers
                .FirstOrDefaultAsync(m => m.ServerId == id);
            if (gameServers == null) return NotFound();

            return View(gameServers);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind(
                "Title,GameType,Hostname,QueryPort,FtpHostname,FtpUsername,FtpPassword,RconPassword,LiveTitle,LiveMap,LiveMod,LiveMaxPlayers,LiveCurrentPlayers,LiveLastUpdated,ShowOnBannerServerList,BannerServerListPosition,HtmlBanner,ShowOnPortalServerList,ShowChatLog")]
            GameServers gameServers)
        {
            if (ModelState.IsValid)
            {
                gameServers.ServerId = Guid.NewGuid();
                _context.Add(gameServers);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(gameServers);
        }

        // GET: GameServers/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var gameServers = await _context.GameServers.FindAsync(id);
            if (gameServers == null) return NotFound();
            return View(gameServers);
        }

        // POST: GameServers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id,
            [Bind(
                "ServerId,Title,GameType,Hostname,QueryPort,FtpHostname,FtpUsername,FtpPassword,RconPassword,LiveTitle,LiveMap,LiveMod,LiveMaxPlayers,LiveCurrentPlayers,LiveLastUpdated,ShowOnBannerServerList,BannerServerListPosition,HtmlBanner,ShowOnPortalServerList,ShowChatLog")]
            GameServers gameServers)
        {
            if (id != gameServers.ServerId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(gameServers);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GameServersExists(gameServers.ServerId))
                        return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(gameServers);
        }

        // GET: GameServers/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var gameServers = await _context.GameServers
                .FirstOrDefaultAsync(m => m.ServerId == id);
            if (gameServers == null) return NotFound();

            return View(gameServers);
        }

        // POST: GameServers/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var gameServers = await _context.GameServers.FindAsync(id);
            _context.GameServers.Remove(gameServers);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GameServersExists(Guid id)
        {
            return _context.GameServers.Any(e => e.ServerId == id);
        }
    }
}