﻿using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApiClient.V1;

namespace XtremeIdiots.Portal.Web.ViewComponents
{
    public class GameServerListViewComponent : ViewComponent
    {
        public GameServerListViewComponent(IRepositoryApiClient repositoryApiClient)
        {
            this.repositoryApiClient = repositoryApiClient;
        }

        public IRepositoryApiClient repositoryApiClient { get; }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var gameServersApiResponse = await repositoryApiClient.GameServers.V1.GetGameServers(null, null, GameServerFilter.BannerServerListEnabled, 0, 50, GameServerOrder.BannerServerListPosition);

            var filtered = gameServersApiResponse.Result.Entries.Where(s => !string.IsNullOrWhiteSpace(s.HtmlBanner)).ToList();

            return View(filtered);
        }
    }
}