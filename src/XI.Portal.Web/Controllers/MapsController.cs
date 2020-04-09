using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using XI.Portal.Data.Legacy.CommonTypes;
using XI.Portal.Maps.Models;
using XI.Portal.Maps.Repository;
using XI.Portal.Web.Models;

namespace XI.Portal.Web.Controllers
{
    [AllowAnonymous]
    public class MapsController : Controller
    {
        private readonly IMapsRepository _mapsRepository;
        private readonly IMapImageRepository _mapImageRepository;
        private readonly IMapFileRepository _mapFileRepository;

        public MapsController(IMapsRepository mapsRepository, IMapImageRepository mapImageRepository, IMapFileRepository mapFileRepository)
        {
            _mapsRepository = mapsRepository ?? throw new ArgumentNullException(nameof(mapsRepository));
            _mapImageRepository = mapImageRepository ?? throw new ArgumentNullException(nameof(mapImageRepository));
            _mapFileRepository = mapFileRepository ?? throw new ArgumentNullException(nameof(mapFileRepository));
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> GetGlobalMapListAjax()
        {
            var reader = new StreamReader(Request.Body);
            var requestBody = await reader.ReadToEndAsync();

            var model = JsonConvert.DeserializeObject<DataTableAjaxPostModel>(requestBody);

            if (model == null)
                return BadRequest();

            var mapsFilterModel = new MapsFilterModel();
            var recordsTotal = await _mapsRepository.GetMapListCount(mapsFilterModel);

            mapsFilterModel.FilterString = model.Search?.Value;
            var recordsFiltered = await _mapsRepository.GetMapListCount(mapsFilterModel);

            mapsFilterModel.TakeEntries = model.Length;
            mapsFilterModel.SkipEntries = model.Start;

            if (model.Order == null)
            {
                mapsFilterModel.Order = MapsFilterModel.OrderBy.MapNameAsc;
            }
            else
            {
                var orderColumn = model.Columns[model.Order.First().Column].Name;
                var searchOrder = model.Order.First().Dir;

                switch (orderColumn)
                {
                    case "mapName":
                        mapsFilterModel.Order = searchOrder == "asc" ? MapsFilterModel.OrderBy.MapNameAsc : MapsFilterModel.OrderBy.MapNameDesc;
                        break;
                    case "popularity":
                        mapsFilterModel.Order = searchOrder == "asc" ? MapsFilterModel.OrderBy.LikeDislikeAsc : MapsFilterModel.OrderBy.LikeDislikeDesc;
                        break;
                    case "gameType":
                        mapsFilterModel.Order = searchOrder == "asc" ? MapsFilterModel.OrderBy.GameTypeAsc : MapsFilterModel.OrderBy.GameTypeDesc;
                        break;
                }
            }

            var mapListEntries = await _mapsRepository.GetMapList(mapsFilterModel);

            return Json(new
            {
                model.Draw,
                recordsTotal,
                recordsFiltered,
                data = mapListEntries
            });
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> DownloadFullRotation(Guid? id)
        {
            if (id == null) return NotFound();

            var fullRotationArchive = await _mapFileRepository.GetFullRotationArchive((Guid) id);

            return File(fullRotationArchive, "application/zip");
        }

        [HttpGet]
        public async Task<ActionResult> MapImage(GameType gameType, string mapName)
        {
            if (gameType == GameType.Unknown || string.IsNullOrWhiteSpace(mapName))
                return BadRequest();

            var mapImage = await _mapImageRepository.GetMapImage(gameType, mapName);

            return Redirect(mapImage.ToString());
        }
    }
}