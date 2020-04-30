using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using XI.CommonTypes;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Contract.Extensions;
using XI.Portal.Auth.FileMonitors.Extensions;
using XI.Portal.Servers.Dto;
using XI.Portal.Servers.Extensions;
using XI.Portal.Servers.Interfaces;
using XI.Portal.Servers.Models;
using XI.Portal.Web.Extensions;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = XtremeIdiotsPolicy.AccessFileMonitors)]
    public class FileMonitorsController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IFileMonitorsRepository _fileMonitorsRepository;
        private readonly IGameServersRepository _gameServersRepository;
        private readonly ILogger<FileMonitorsController> _logger;

        public FileMonitorsController(
            ILogger<FileMonitorsController> logger,
            IAuthorizationService authorizationService,
            IFileMonitorsRepository fileMonitorsRepository,
            IGameServersRepository gameServersRepository)
        {
            _fileMonitorsRepository = fileMonitorsRepository ?? throw new ArgumentNullException(nameof(fileMonitorsRepository));
            _gameServersRepository = gameServersRepository ?? throw new ArgumentNullException(nameof(gameServersRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var filterModel = new FileMonitorFilterModel
            {
                Order = FileMonitorFilterModel.OrderBy.BannerServerListPosition
            }.ApplyAuth(User);
            var fileMonitorsDtos = await _fileMonitorsRepository.GetFileMonitors(filterModel);

            return View(fileMonitorsDtos);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await AddGameServersViewData();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FileMonitorDto model)
        {
            var gameServerDto = await _gameServersRepository.GetGameServer(model.ServerId);
            if (gameServerDto == null) return NotFound();

            if (!ModelState.IsValid)
            {
                await AddGameServersViewData(model.ServerId);
                return View(model);
            }

            var fileMonitorDto = new FileMonitorDto().WithServerDto(gameServerDto);
            var canCreateFileMonitor = await _authorizationService.AuthorizeAsync(User, fileMonitorDto, XtremeIdiotsPolicy.CreateFileMonitor);

            if (!canCreateFileMonitor.Succeeded)
                return Unauthorized();

            fileMonitorDto.FilePath = model.FilePath;

            await _fileMonitorsRepository.CreateFileMonitor(fileMonitorDto);

            _logger.LogInformation(EventIds.Management, "User {User} has created a new file monitor with Id {Id}", User.Username(), fileMonitorDto.FileMonitorId);
            this.AddAlertSuccess($"The file monitor has been created for {gameServerDto.Title}");

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var fileMonitorDto = await _fileMonitorsRepository.GetFileMonitor(id);
            if (fileMonitorDto == null) return NotFound();

            var canEditFileMonitor = await _authorizationService.AuthorizeAsync(User, fileMonitorDto, XtremeIdiotsPolicy.ViewFileMonitor);

            if (!canEditFileMonitor.Succeeded)
                return Unauthorized();

            return View(fileMonitorDto);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var fileMonitorDto = await _fileMonitorsRepository.GetFileMonitor(id);
            if (fileMonitorDto == null) return NotFound();

            var canEditFileMonitor = await _authorizationService.AuthorizeAsync(User, fileMonitorDto, XtremeIdiotsPolicy.EditFileMonitor);

            if (!canEditFileMonitor.Succeeded)
                return Unauthorized();

            await AddGameServersViewData(fileMonitorDto.ServerId);

            return View(fileMonitorDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(FileMonitorDto model)
        {
            var fileMonitorDto = await _fileMonitorsRepository.GetFileMonitor(model.FileMonitorId);
            if (fileMonitorDto == null) return NotFound();

            if (!ModelState.IsValid)
            {
                await AddGameServersViewData(model.ServerId);
                return View(fileMonitorDto);
            }

            var canEditFileMonitor = await _authorizationService.AuthorizeAsync(User, fileMonitorDto, XtremeIdiotsPolicy.EditFileMonitor);

            if (!canEditFileMonitor.Succeeded)
                return Unauthorized();

            fileMonitorDto.FilePath = model.FilePath;

            await _fileMonitorsRepository.UpdateFileMonitor(fileMonitorDto);

            _logger.LogInformation(EventIds.Management, "User {User} has updated {FileMonitorId} against {ServerId}", User.Username(), fileMonitorDto.FileMonitorId, fileMonitorDto.ServerId);
            this.AddAlertSuccess($"The file monitor has been created for {fileMonitorDto.GameServer.Title}");

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var fileMonitorDto = await _fileMonitorsRepository.GetFileMonitor(id);
            if (fileMonitorDto == null) return NotFound();

            var canDeleteFileMonitor = await _authorizationService.AuthorizeAsync(User, fileMonitorDto, XtremeIdiotsPolicy.DeleteFileMonitor);

            if (!canDeleteFileMonitor.Succeeded)
                return Unauthorized();

            await AddGameServersViewData(fileMonitorDto.ServerId);

            return View(fileMonitorDto);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var fileMonitorDto = await _fileMonitorsRepository.GetFileMonitor(id);
            if (fileMonitorDto == null) return NotFound();

            var canDeleteFileMonitor = await _authorizationService.AuthorizeAsync(User, fileMonitorDto, XtremeIdiotsPolicy.DeleteFileMonitor);

            if (!canDeleteFileMonitor.Succeeded)
                return Unauthorized();

            await _fileMonitorsRepository.DeleteFileMonitor(id);

            _logger.LogInformation(EventIds.Management, "User {User} has deleted {FileMonitorId} against {ServerId}", User.Username(), fileMonitorDto.FileMonitorId, fileMonitorDto.ServerId);
            this.AddAlertSuccess($"The file monitor has been deleted for {fileMonitorDto.GameServer.Title}");

            return RedirectToAction(nameof(Index));
        }

        private async Task AddGameServersViewData(Guid? selected = null)
        {
            var filterModel = new GameServerFilterModel
            {
                Order = GameServerFilterModel.OrderBy.BannerServerListPosition
            }.ApplyAuthForFileMonitors(User);
            var gameServerDtos = await _gameServersRepository.GetGameServers(filterModel);
            ViewData["GameServers"] = new SelectList(gameServerDtos, "ServerId", "Title", selected);
        }
    }
}