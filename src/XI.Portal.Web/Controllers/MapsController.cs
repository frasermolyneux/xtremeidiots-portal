using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using XI.Portal.Web.Auth.Constants;
using XI.Portal.Web.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApiClient;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = AuthPolicies.AccessMaps)]
    public class MapsController : Controller
    {
        private readonly IRepositoryApiClient repositoryApiClient;

        public MapsController(IRepositoryApiClient repositoryApiClient)
        {
            this.repositoryApiClient = repositoryApiClient;
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

            var order = MapsOrder.MapNameAsc;

            var orderColumn = model.Columns[model.Order.First().Column].Name;
            var searchOrder = model.Order.First().Dir;

            switch (orderColumn)
            {
                case "mapName":
                    order = searchOrder == "asc" ? MapsOrder.MapNameAsc : MapsOrder.MapNameDesc;
                    break;
                case "popularity":
                    order = searchOrder == "asc" ? MapsOrder.PopularityAsc : MapsOrder.PopularityDesc;
                    break;
                case "gameType":
                    order = searchOrder == "asc" ? MapsOrder.GameTypeAsc : MapsOrder.GameTypeDesc;
                    break;
            }

            var mapsResponseDto = await repositoryApiClient.Maps.GetMaps(id, null, model.Search?.Value, model.Start, model.Length, order);

            return Json(new
            {
                model.Draw,
                recordsTotal = mapsResponseDto.TotalRecords,
                recordsFiltered = mapsResponseDto.FilteredRecords,
                data = mapsResponseDto.Entries
            });
        }

        [HttpGet]
        public async Task<IActionResult> MapImage(GameType gameType, string mapName)
        {
            if (gameType == GameType.Unknown || string.IsNullOrWhiteSpace(mapName))
                return BadRequest();

            var map = await repositoryApiClient.Maps.GetMap(gameType, mapName);

            if (map == null || string.IsNullOrWhiteSpace(map.MapImageUri))
                return Redirect("/images/noimage.jpg");

            return Redirect(map.MapImageUri);
        }
    }
}