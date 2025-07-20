using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.Models;

namespace XtremeIdiots.Portal.Web.ApiControllers;

/// <summary>
/// API controller providing data endpoints for AJAX requests from the portal interface
/// </summary>
/// <remarks>
/// This controller handles DataTables AJAX requests for various entities including players, maps, and users.
/// All endpoints require appropriate authorization and return data in DataTables-compatible format.
/// </remarks>
[Route("api/[controller]")]
public class DataController : BaseApiController
{
    private readonly IRepositoryApiClient repositoryApiClient;

    /// <summary>
    /// Initializes a new instance of the DataController
    /// </summary>
    /// <param name="repositoryApiClient">Client for repository API operations</param>
    /// <param name="telemetryClient">Client for application telemetry</param>
    /// <param name="logger">Logger instance for this controller</param>
    /// <param name="configuration">Application configuration</param>
    /// <exception cref="ArgumentNullException">Thrown when required dependencies are null</exception>
    public DataController(
        IRepositoryApiClient repositoryApiClient,
        TelemetryClient telemetryClient,
        ILogger<DataController> logger,
        IConfiguration configuration)
        : base(telemetryClient, logger, configuration)
    {
        this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
    }

    /// <summary>
    /// Retrieves players data for DataTables AJAX requests with username and GUID filtering
    /// </summary>
    /// <param name="id">Optional game type to filter players by</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>DataTables-compatible JSON response with player data</returns>
    [HttpPost("Players/GetPlayersAjax")]
    [Authorize(Policy = AuthPolicies.AccessPlayers)]
    public async Task<IActionResult> GetPlayersAjax(GameType? id, CancellationToken cancellationToken = default)
    {
        return await GetPlayersAjaxPrivate(PlayersFilter.UsernameAndGuid, id, cancellationToken);
    }

    /// <summary>
    /// Retrieves players data for IP address search functionality
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>DataTables-compatible JSON response with player IP data</returns>
    [HttpPost("Players/GetIpSearchListAjax")]
    [Authorize(Policy = AuthPolicies.AccessPlayers)]
    public async Task<IActionResult> GetIpSearchListAjax(CancellationToken cancellationToken = default)
    {
        return await GetPlayersAjaxPrivate(PlayersFilter.IpAddress, null, cancellationToken);
    }

    /// <summary>
    /// Retrieves maps data for DataTables AJAX requests with sorting and filtering support
    /// </summary>
    /// <param name="id">Optional game type to filter maps by</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>DataTables-compatible JSON response with map data</returns>
    [HttpPost("Maps/GetMapListAjax")]
    [Authorize(Policy = AuthPolicies.AccessMaps)]
    public async Task<IActionResult> GetMapListAjax(GameType? id, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var reader = new StreamReader(Request.Body);
            var requestBody = await reader.ReadToEndAsync();

            var model = JsonConvert.DeserializeObject<DataTableAjaxPostModel>(requestBody);

            if (model is null)
            {
                Logger.LogWarning("Invalid DataTable request body for user {UserId}", User.XtremeIdiotsId());
                return BadRequest();
            }

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

            var mapsApiResponse = await repositoryApiClient.Maps.V1.GetMaps(id, null, null, model.Search?.Value, model.Start, model.Length, order);

            if (!mapsApiResponse.IsSuccess || mapsApiResponse.Result?.Data is null)
            {
                Logger.LogWarning("Failed to retrieve maps data for user {UserId} and game type {GameType}",
                    User.XtremeIdiotsId(), id);
                return StatusCode(500, "Failed to retrieve maps data");
            }

            TrackSuccessTelemetry("MapsListRetrieved", nameof(GetMapListAjax), new Dictionary<string, string>
            {
                { "GameType", id?.ToString() ?? "All" },
                { "ResultCount", mapsApiResponse.Result.Data.Items?.Count().ToString() ?? "0" }
            });

            return Ok(new
            {
                model.Draw,
                recordsTotal = mapsApiResponse.Result.Data.TotalCount,
                recordsFiltered = mapsApiResponse.Result.Data.FilteredCount,
                data = mapsApiResponse.Result.Data.Items
            });
        }, nameof(GetMapListAjax));
    }

    /// <summary>
    /// Retrieves user profiles data for DataTables AJAX requests
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>DataTables-compatible JSON response with user profile data</returns>
    [HttpPost("Users/GetUsersAjax")]
    [Authorize(Policy = AuthPolicies.AccessUsers)]
    public async Task<IActionResult> GetUsersAjax(CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var reader = new StreamReader(Request.Body);
            var requestBody = await reader.ReadToEndAsync(cancellationToken);

            var model = JsonConvert.DeserializeObject<DataTableAjaxPostModel>(requestBody);

            if (model is null)
            {
                Logger.LogWarning("Invalid request body for users AJAX endpoint");
                return BadRequest();
            }

            var userProfileResponseDto = await repositoryApiClient.UserProfiles.V1.GetUserProfiles(
                model.Search?.Value, model.Start, model.Length, UserProfilesOrder.DisplayNameAsc, cancellationToken);

            if (userProfileResponseDto.Result?.Data is null)
            {
                Logger.LogWarning("Invalid API response for users AJAX endpoint");
                return BadRequest();
            }

            return Ok(new
            {
                model.Draw,
                recordsTotal = userProfileResponseDto.Result.Data.TotalCount,
                recordsFiltered = userProfileResponseDto.Result.Data.FilteredCount,
                data = userProfileResponseDto.Result.Data.Items
            });
        }, nameof(GetUsersAjax));
    }

    /// <summary>
    /// Private helper method to handle different types of player AJAX requests with filtering and sorting
    /// </summary>
    /// <param name="filter">The filter type to apply to player queries</param>
    /// <param name="gameType">Optional game type to filter players by</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>DataTables-compatible JSON response with filtered player data</returns>
    private async Task<IActionResult> GetPlayersAjaxPrivate(PlayersFilter filter, GameType? gameType, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var reader = new StreamReader(Request.Body);
            var requestBody = await reader.ReadToEndAsync(cancellationToken);

            var model = JsonConvert.DeserializeObject<DataTableAjaxPostModel>(requestBody);

            if (model is null)
            {
                Logger.LogWarning("Invalid request model for players AJAX from user {UserId}", User.XtremeIdiotsId());
                return BadRequest();
            }

            var order = PlayersOrder.LastSeenDesc;

            if (model.Order?.Any() == true)
            {
                var orderColumn = model.Columns[model.Order.First().Column].Name;
                var searchOrder = model.Order.First().Dir;

                switch (orderColumn)
                {
                    case "username":
                        order = searchOrder == "asc" ? PlayersOrder.UsernameAsc : PlayersOrder.UsernameDesc;
                        break;
                    case "gameType":
                        order = searchOrder == "asc" ? PlayersOrder.GameTypeAsc : PlayersOrder.GameTypeDesc;
                        break;
                    case "firstSeen":
                        order = searchOrder == "asc" ? PlayersOrder.FirstSeenAsc : PlayersOrder.FirstSeenDesc;
                        break;
                    case "lastSeen":
                        order = searchOrder == "asc" ? PlayersOrder.LastSeenAsc : PlayersOrder.LastSeenDesc;
                        break;
                }
            }

            var playersApiResponse = await repositoryApiClient.Players.V1.GetPlayers(
                gameType, filter, model.Search?.Value, model.Start, model.Length, order, PlayerEntityOptions.None);

            if (!playersApiResponse.IsSuccess || playersApiResponse.Result?.Data is null)
            {
                Logger.LogError("Failed to retrieve players for user {UserId} with filter {Filter}",
                    User.XtremeIdiotsId(), filter);
                return StatusCode(500, "Failed to retrieve players data");
            }

            TrackSuccessTelemetry("PlayersListLoaded", nameof(GetPlayersAjax), new Dictionary<string, string>
            {
                { "Filter", filter.ToString() },
                { "GameType", gameType?.ToString() ?? "All" },
                { "ResultCount", playersApiResponse.Result.Data.Items?.Count().ToString() ?? "0" }
            });

            return Ok(new
            {
                model.Draw,
                recordsTotal = playersApiResponse.Result.Data.TotalCount,
                recordsFiltered = playersApiResponse.Result.Data.FilteredCount,
                data = playersApiResponse.Result.Data.Items
            });
        }, nameof(GetPlayersAjax), $"filter: {filter}, gameType: {gameType}");
    }
}