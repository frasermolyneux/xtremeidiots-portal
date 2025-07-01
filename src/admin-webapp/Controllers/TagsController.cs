using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using XtremeIdiots.Portal.AdminWebApp.Auth.Constants;
using XtremeIdiots.Portal.AdminWebApp.Extensions;
using XtremeIdiots.Portal.AdminWebApp.Models;
using XtremeIdiots.Portal.AdminWebApp.ViewModels;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Tags;
using XtremeIdiots.Portal.RepositoryApiClient.V1;

namespace XtremeIdiots.Portal.AdminWebApp.Controllers
{
    [Authorize(Policy = AuthPolicies.AccessPlayerTags)]
    public class TagsController : Controller
    {
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly TelemetryClient telemetryClient;

        public TagsController(
            IRepositoryApiClient repositoryApiClient,
            TelemetryClient telemetryClient)
        {
            this.repositoryApiClient = repositoryApiClient;
            this.telemetryClient = telemetryClient;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var tagsResponse = await repositoryApiClient.Tags.V1.GetTags(0, 100);

            if (!tagsResponse.IsSuccess || tagsResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            var model = new TagsViewModel
            {
                Tags = tagsResponse.Result.Entries
            };

            return View(model);
        }

        [HttpGet]
        [Authorize(Policy = AuthPolicies.CreatePlayerTag)]
        public IActionResult Create()
        {
            return View(new CreateTagViewModel());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = AuthPolicies.CreatePlayerTag)]
        public async Task<IActionResult> Create(CreateTagViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);            // Allow setting UserDefined property when creating tags
            var createTagDto = new TagDto
            {
                Name = model.Name,
                Description = model.Description,
                TagHtml = model.TagHtml,
                UserDefined = model.UserDefined // Use the value from the form
            };

            var response = await repositoryApiClient.Tags.V1.CreateTag(createTagDto);

            if (!response.IsSuccess)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            var eventTelemetry = new Microsoft.ApplicationInsights.DataContracts.EventTelemetry("TagCreated").Enrich(User);
            eventTelemetry.Properties.Add("TagName", model.Name);
            telemetryClient.TrackEvent(eventTelemetry);

            this.AddAlertSuccess($"The tag '{model.Name}' has been successfully created");

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Policy = AuthPolicies.EditPlayerTag)]
        public async Task<IActionResult> Edit(Guid id)
        {
            var tagResponse = await repositoryApiClient.Tags.V1.GetTag(id);

            if (!tagResponse.IsSuccess || tagResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 404 }); var model = new EditTagViewModel
                {
                    TagId = tagResponse.Result.TagId,
                    Name = tagResponse.Result.Name,
                    Description = tagResponse.Result.Description,
                    TagHtml = tagResponse.Result.TagHtml,
                    UserDefined = tagResponse.Result.UserDefined
                };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = AuthPolicies.EditPlayerTag)]
        public async Task<IActionResult> Edit(EditTagViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);            // First get the original tag to check if it's user defined
            var originalTagResponse = await repositoryApiClient.Tags.V1.GetTag(model.TagId);

            if (!originalTagResponse.IsSuccess || originalTagResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 404 });            // Allow changing the UserDefined status
            var tagDto = new TagDto
            {
                TagId = model.TagId,
                Name = model.Name,
                Description = model.Description,
                TagHtml = model.TagHtml,
                UserDefined = model.UserDefined // Use the value from the form
            };

            var response = await repositoryApiClient.Tags.V1.UpdateTag(tagDto);

            if (!response.IsSuccess)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            var eventTelemetry = new Microsoft.ApplicationInsights.DataContracts.EventTelemetry("TagUpdated").Enrich(User);
            eventTelemetry.Properties.Add("TagName", model.Name);
            eventTelemetry.Properties.Add("TagId", model.TagId.ToString());
            telemetryClient.TrackEvent(eventTelemetry);

            this.AddAlertSuccess($"The tag '{model.Name}' has been successfully updated");

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Policy = AuthPolicies.DeletePlayerTag)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var tagResponse = await repositoryApiClient.Tags.V1.GetTag(id);

            if (!tagResponse.IsSuccess || tagResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 404 });

            return View(tagResponse.Result);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = AuthPolicies.DeletePlayerTag)]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var tagResponse = await repositoryApiClient.Tags.V1.GetTag(id);

            if (!tagResponse.IsSuccess || tagResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 404 });

            // Check if the user has permission to delete this tag based on UserDefined property
            if (!tagResponse.Result.UserDefined)
            {
                // Only senior admins can delete non-UserDefined tags
                if (!User.HasClaim(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
                {
                    this.AddAlertDanger($"You do not have permission to delete the system tag '{tagResponse.Result.Name}'");
                    return RedirectToAction(nameof(Index));
                }
            }
            // For UserDefined tags, the authorization is already handled by the policy

            var response = await repositoryApiClient.Tags.V1.DeleteTag(id);

            if (!response.IsSuccess)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            var eventTelemetry = new Microsoft.ApplicationInsights.DataContracts.EventTelemetry("TagDeleted").Enrich(User);
            eventTelemetry.Properties.Add("TagName", tagResponse.Result.Name);
            eventTelemetry.Properties.Add("TagId", id.ToString());
            telemetryClient.TrackEvent(eventTelemetry);

            this.AddAlertSuccess($"The tag '{tagResponse.Result.Name}' has been successfully deleted");

            return RedirectToAction(nameof(Index));
        }
    }
}
