using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using XI.CommonTypes;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Demos.Dto;
using XI.Portal.Demos.Interfaces;
using XI.Portal.Demos.Models;
using XI.Portal.Web.Models;

namespace XI.Portal.Web.Controllers
{
    [Authorize]
    public class DemosController : Controller
    {
        private readonly IDemosRepository _demosRepository;

        private readonly string[] _requiredClaims = {XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin, XtremeIdiotsClaimTypes.GameAdmin, XtremeIdiotsClaimTypes.Moderator};

        public DemosController(IDemosRepository demosRepository)
        {
            _demosRepository = demosRepository ?? throw new ArgumentNullException(nameof(demosRepository));
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GameIndex(GameType? id)
        {
            ViewData["GameType"] = id;
            return View(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> GetDemoListAjax(GameType? id)
        {
            var reader = new StreamReader(Request.Body);
            var requestBody = await reader.ReadToEndAsync();

            var model = JsonConvert.DeserializeObject<DataTableAjaxPostModel>(requestBody);

            if (model == null)
                return BadRequest();

            var filterModel = new DemosFilterModel();

            if (id != null)
                filterModel.GameType = (GameType) id;

            var recordsTotal = await _demosRepository.GetDemoCount(filterModel, User, _requiredClaims);

            filterModel.FilterString = model.Search?.Value;
            var recordsFiltered = await _demosRepository.GetDemoCount(filterModel, User, _requiredClaims);

            filterModel.TakeEntries = model.Length;
            filterModel.SkipEntries = model.Start;

            if (model.Order == null)
            {
                filterModel.Order = DemosFilterModel.OrderBy.DateDesc;
            }
            else
            {
                var orderColumn = model.Columns[model.Order.First().Column].Name;
                var searchOrder = model.Order.First().Dir;

                switch (orderColumn)
                {
                    case "game":
                        filterModel.Order = searchOrder == "asc" ? DemosFilterModel.OrderBy.GameTypeAsc : DemosFilterModel.OrderBy.GameTypeDesc;
                        break;
                    case "name":
                        filterModel.Order = searchOrder == "asc" ? DemosFilterModel.OrderBy.NameAsc : DemosFilterModel.OrderBy.NameDesc;
                        break;
                    case "date":
                        filterModel.Order = searchOrder == "asc" ? DemosFilterModel.OrderBy.DateAsc : DemosFilterModel.OrderBy.DateDesc;
                        break;
                    case "uploadedBy":
                        filterModel.Order = searchOrder == "asc" ? DemosFilterModel.OrderBy.UploadedByAsc : DemosFilterModel.OrderBy.UploadedByDesc;
                        break;
                }
            }

            var demosEntries = await _demosRepository.GetDemos(filterModel, User, _requiredClaims);
            var portalDemoEntries = demosEntries.Select(demo => new PortalDemoDto(demo)).ToList();

            foreach (var portalDemoEntry in portalDemoEntries)
            {
                var canDeletePortalDemo = User.Claims.Any(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin ||
                                                                   claim.Type == XtremeIdiotsClaimTypes.HeadAdmin && claim.Value == portalDemoEntry.GameType ||
                                                                   claim.Type == XtremeIdiotsClaimTypes.XtremeIdiotsId && claim.Value == portalDemoEntry.UserId);

                if (canDeletePortalDemo)
                    portalDemoEntry.ShowDeleteLink = true;
            }

            return Json(new
            {
                model.Draw,
                recordsTotal,
                recordsFiltered,
                data = portalDemoEntries
            });
        }

        public class PortalDemoDto
        {
            public PortalDemoDto(DemoDto demo)
            {
                DemoId = demo.DemoId;
                Game = demo.Game;
                Name = demo.Name;
                FileName = demo.FileName;
                Date = demo.Date;
                Map = demo.Map;
                Mod = demo.Mod;
                GameType = demo.GameType;
                Server = demo.Server;
                Size = demo.Size;
                UserId = demo.UserId;
                UploadedBy = demo.UploadedBy;
            }

            public Guid DemoId { get; set; }
            public string Game { get; set; }
            public string Name { get; set; }
            public string FileName { get; set; }
            public DateTime Date { get; set; }
            public string Map { get; set; }
            public string Mod { get; set; }
            public string GameType { get; set; }
            public string Server { get; set; }
            public long Size { get; set; }
            public string UserId { get; set; }
            public string UploadedBy { get; set; }

            public bool ShowDeleteLink { get; set; }
        }
    }
}