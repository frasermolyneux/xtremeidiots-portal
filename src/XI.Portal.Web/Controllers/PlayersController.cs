using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using XI.Portal.Players.Models;
using XI.Portal.Players.Repository;
using XI.Portal.Web.Constants;
using XI.Portal.Web.Models;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = XtremeIdiotsPolicy.Admin)]
    public class PlayersController : Controller
    {
        private readonly IPlayersRepository _playersRepository;

        public PlayersController(IPlayersRepository playersRepository)
        {
            _playersRepository = playersRepository ?? throw new ArgumentNullException(nameof(playersRepository));
        }

        [HttpGet]
        public IActionResult GlobalPlayers()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetGlobalPlayersListAjax()
        {
            var reader = new StreamReader(Request.Body);
            var requestBody = await reader.ReadToEndAsync();

            var model = JsonConvert.DeserializeObject<DataTableAjaxPostModel>(requestBody);

            if (model == null)
                return BadRequest();

            var filterModel = new PlayersFilterModel
            {
                Filter = PlayersFilterModel.FilterType.UsernameAndGuid
            };
            var recordsTotal = await _playersRepository.GetPlayerListCount(filterModel);

            filterModel.FilterString = model.Search?.Value;
            var recordsFiltered = await _playersRepository.GetPlayerListCount(filterModel);

            filterModel.TakeEntries = model.Length;
            filterModel.SkipEntries = model.Start;

            if (model.Order == null)
            {
                filterModel.Order = PlayersFilterModel.OrderBy.LastSeenDesc;
            }
            else
            {
                var orderColumn = model.Columns[model.Order.First().Column].Name;
                var searchOrder = model.Order.First().Dir;

                switch (orderColumn)
                {
                    case "gameType":
                        filterModel.Order = searchOrder == "asc" ? PlayersFilterModel.OrderBy.GameTypeAsc : PlayersFilterModel.OrderBy.GameTypeDesc;
                        break;
                    case "username":
                        filterModel.Order = searchOrder == "asc" ? PlayersFilterModel.OrderBy.UsernameAsc : PlayersFilterModel.OrderBy.UsernameDesc;
                        break;
                    case "firstSeen":
                        filterModel.Order = searchOrder == "asc" ? PlayersFilterModel.OrderBy.FirstSeenAsc : PlayersFilterModel.OrderBy.FirstSeenDesc;
                        break;
                    case "lastSeen":
                        filterModel.Order = searchOrder == "asc" ? PlayersFilterModel.OrderBy.LastSeenAsc : PlayersFilterModel.OrderBy.LastSeenDesc;
                        break;
                }
            }

            var playersListEntries = await _playersRepository.GetPlayerList(filterModel);

            return Json(new
            {
                model.Draw,
                recordsTotal,
                recordsFiltered,
                data = playersListEntries
            });
        }
    }
}