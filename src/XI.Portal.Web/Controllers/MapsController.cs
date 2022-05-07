using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Maps.Interfaces;
using XI.Portal.Repository.Interfaces;
using XI.Portal.Repository.Models;
using XI.Portal.Web.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = AuthPolicies.AccessMaps)]
    public class MapsController : Controller
    {
        private readonly IMapImageRepository _mapImageRepository;
        private readonly IMapsRepository _mapsRepository;

        public MapsController(
            IMapsRepository mapsRepository,
            IMapImageRepository mapImageRepository)
        {
            _mapsRepository = mapsRepository ?? throw new ArgumentNullException(nameof(mapsRepository));
            _mapImageRepository = mapImageRepository ?? throw new ArgumentNullException(nameof(mapImageRepository));
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
        public async Task<IActionResult> GetMapListAjax(GameType? id)
        {
            var reader = new StreamReader(Request.Body);
            var requestBody = await reader.ReadToEndAsync();

            var model = JsonConvert.DeserializeObject<DataTableAjaxPostModel>(requestBody);

            if (model == null)
                return BadRequest();

            var queryOptions = new MapsQueryOptions();

            if (id != null)
                queryOptions.GameType = (GameType)id;

            var recordsTotal = await _mapsRepository.GetMapsCount(queryOptions);

            queryOptions.FilterString = model.Search?.Value;
            var recordsFiltered = await _mapsRepository.GetMapsCount(queryOptions);

            queryOptions.TakeEntries = model.Length;
            queryOptions.SkipEntries = model.Start;

            if (model.Order == null)
            {
                queryOptions.Order = MapsQueryOptions.OrderBy.MapNameAsc;
            }
            else
            {
                var orderColumn = model.Columns[model.Order.First().Column].Name;
                var searchOrder = model.Order.First().Dir;

                switch (orderColumn)
                {
                    case "mapName":
                        queryOptions.Order = searchOrder == "asc" ? MapsQueryOptions.OrderBy.MapNameAsc : MapsQueryOptions.OrderBy.MapNameDesc;
                        break;
                    case "popularity":
                        queryOptions.Order = searchOrder == "asc" ? MapsQueryOptions.OrderBy.LikeDislikeAsc : MapsQueryOptions.OrderBy.LikeDislikeDesc;
                        break;
                    case "gameType":
                        queryOptions.Order = searchOrder == "asc" ? MapsQueryOptions.OrderBy.GameTypeAsc : MapsQueryOptions.OrderBy.GameTypeDesc;
                        break;
                }
            }

            var mapDtos = await _mapsRepository.GetMaps(queryOptions);

            return Json(new
            {
                model.Draw,
                recordsTotal,
                recordsFiltered,
                data = mapDtos
            });
        }

        [HttpGet]
        public async Task<IActionResult> MapImage(GameType gameType, string mapName)
        {
            if (gameType == GameType.Unknown || string.IsNullOrWhiteSpace(mapName))
                return BadRequest();

            var mapImage = await _mapImageRepository.GetMapImage(gameType, mapName);

            return Redirect(mapImage.ToString());
        }
    }
}