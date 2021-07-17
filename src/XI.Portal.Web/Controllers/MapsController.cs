using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using XI.CommonTypes;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Maps.Dto;
using XI.Portal.Maps.Interfaces;
using XI.Portal.Maps.Models;
using XI.Portal.Web.Models;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = AuthPolicies.AccessMaps)]
    public class MapsController : Controller
    {
        private readonly IMapFileRepository _mapFileRepository;
        private readonly IMapImageRepository _mapImageRepository;
        private readonly IMapsRepository _mapsRepository;

        public MapsController(IMapsRepository mapsRepository, IMapImageRepository mapImageRepository, IMapFileRepository mapFileRepository)
        {
            _mapsRepository = mapsRepository ?? throw new ArgumentNullException(nameof(mapsRepository));
            _mapImageRepository = mapImageRepository ?? throw new ArgumentNullException(nameof(mapImageRepository));
            _mapFileRepository = mapFileRepository ?? throw new ArgumentNullException(nameof(mapFileRepository));
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

            var filterModel = new MapsFilterModel();

            if (id != null)
                filterModel.GameType = (GameType) id;

            var recordsTotal = await _mapsRepository.GetMapsCount(filterModel);

            filterModel.FilterString = model.Search?.Value;
            var recordsFiltered = await _mapsRepository.GetMapsCount(filterModel);

            filterModel.TakeEntries = model.Length;
            filterModel.SkipEntries = model.Start;

            if (model.Order == null)
            {
                filterModel.Order = MapsFilterModel.OrderBy.MapNameAsc;
            }
            else
            {
                var orderColumn = model.Columns[model.Order.First().Column].Name;
                var searchOrder = model.Order.First().Dir;

                switch (orderColumn)
                {
                    case "mapName":
                        filterModel.Order = searchOrder == "asc" ? MapsFilterModel.OrderBy.MapNameAsc : MapsFilterModel.OrderBy.MapNameDesc;
                        break;
                    //case "popularity":
                    //    filterModel.Order = searchOrder == "asc" ? MapsFilterModel.OrderBy.LikeDislikeAsc : MapsFilterModel.OrderBy.LikeDislikeDesc;
                    //    break;
                    case "gameType":
                        filterModel.Order = searchOrder == "asc" ? MapsFilterModel.OrderBy.GameTypeAsc : MapsFilterModel.OrderBy.GameTypeDesc;
                        break;
                }
            }

            var mapDtos = await _mapsRepository.GetMaps(filterModel);
            var portalMapDtos = mapDtos.Select(m => new PortalMapDto(m)).ToList(); 

            return Json(new
            {
                model.Draw,
                recordsTotal,
                recordsFiltered,
                data = portalMapDtos
            });
        }

        [HttpGet]
        public async Task<IActionResult> DownloadFullRotation(Guid? id)
        {
            if (id == null) return NotFound();

            var fullRotationArchive = await _mapFileRepository.GetFullRotationArchive((Guid) id);

            return File(fullRotationArchive, "application/zip");
        }

        [HttpGet]
        public async Task<IActionResult> MapImage(GameType gameType, string mapName)
        {
            if (gameType == GameType.Unknown || string.IsNullOrWhiteSpace(mapName))
                return BadRequest();

            var mapImage = await _mapImageRepository.GetMapImage(gameType, mapName);

            return Redirect(mapImage.ToString());
        }

        public class PortalMapDto
        {
            public PortalMapDto(MapDto mapDto)
            {
                MapId = mapDto.MapId;
                GameType = mapDto.GameType.ToString();
                MapName = mapDto.MapName;
                LikePercentage = mapDto.LikePercentage;
                DislikePercentage = mapDto.DislikePercentage;
                TotalLikes = mapDto.TotalLikes;
                TotalDislikes = mapDto.TotalDislikes;
                TotalVotes = mapDto.TotalVotes;
                MapFiles = mapDto.MapFiles;
            }

            public Guid MapId { get; set; }
            public string GameType { get; set; }
            public string MapName { get; set; }
            public double LikePercentage { get; set; }
            public double DislikePercentage { get; set; }
            public double TotalLikes { get; set; }
            public double TotalDislikes { get; set; }
            public int TotalVotes { get; set; }
            public IList<MapFileDto> MapFiles { get; set; }
        }
    }
}