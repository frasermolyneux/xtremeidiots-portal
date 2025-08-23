using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Web.Auth.Constants;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Provides reusable user/admin search endpoints for autocomplete scenarios.
/// </summary>
[Authorize(Policy = AuthPolicies.PerformUserSearch)]
public class UserSearchController(IRepositoryApiClient repositoryApiClient, ILogger<UserSearchController> logger) : Controller
{
    private readonly IRepositoryApiClient repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
    private readonly ILogger<UserSearchController> logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Searches user profiles by display name returning lightweight id/text pairs for autocomplete.
    /// </summary>
    /// <param name="term">Partial display name (min 2 chars)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>JSON array of objects: { id, text }</returns>
    [HttpGet]
    public async Task<IActionResult> Users(string? term, CancellationToken cancellationToken = default)
    {
        var search = string.IsNullOrWhiteSpace(term) ? null : term.Trim();
        if (search is null || search.Length < 2)
            return Json(Array.Empty<object>());

        var response = await repositoryApiClient.UserProfiles.V1.GetUserProfiles(search, 0, 15, Repository.Abstractions.Constants.V1.UserProfilesOrder.DisplayNameAsc, cancellationToken);
        if (!response.IsSuccess || response.Result?.Data?.Items is null)
        {
            logger.LogWarning("UserSearch.Users failed for term {Term}", term);
            return Json(Array.Empty<object>());
        }

        var data = response.Result.Data.Items
            .Where(u => !string.IsNullOrWhiteSpace(u.DisplayName))
            .Select(u => new { id = u.XtremeIdiotsForumId ?? u.UserProfileId.ToString(), text = u.DisplayName })
            .ToArray();

        return Json(data);
    }
}
