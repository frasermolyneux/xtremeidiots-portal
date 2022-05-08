﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System.Net;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.BanFileMonitorsApi
{
    public class BanFileMonitorsApiClient : BaseApiClient, IBanFileMonitorsApiClient
    {
        public BanFileMonitorsApiClient(ILogger<BanFileMonitorsApiClient> logger, IOptions<RepositoryApiClientOptions> options) : base(logger, options)
        {
        }

        public async Task<BanFileMonitorDto> GetBanFileMonitor(string accessToken, Guid banFileMonitorId)
        {
            var request = CreateRequest($"repository/ban-file-monitors/{banFileMonitorId}", Method.Get, accessToken);
            var response = await ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (response.Content != null)
                return JsonConvert.DeserializeObject<BanFileMonitorDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<List<BanFileMonitorDto>> GetBanFileMonitors(string accessToken, string[] gameTypes, Guid[] banFileMonitorIds, Guid? serverId, int skipEntries, int takeEntries, string order)
        {
            var request = CreateRequest("repository/ban-file-monitors", Method.Get, accessToken);

            if (gameTypes != null)
                request.AddQueryParameter("gameTypes", string.Join(",", gameTypes));

            if (banFileMonitorIds != null)
                request.AddQueryParameter("banFileMonitorIds", string.Join(",", banFileMonitorIds));

            if (serverId != null)
                request.AddQueryParameter("serverId", serverId.ToString());

            request.AddQueryParameter("takeEntries", takeEntries.ToString());
            request.AddQueryParameter("skipEntries", skipEntries.ToString());

            if (!string.IsNullOrWhiteSpace(order))
                request.AddQueryParameter("order", order);

            var response = await ExecuteAsync(request);

            if (response.Content != null)
                return JsonConvert.DeserializeObject<List<BanFileMonitorDto>>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<BanFileMonitorDto> UpdateBanFileMonitor(string accessToken, BanFileMonitorDto banFileMonitor)
        {
            var request = CreateRequest($"repository/ban-file-monitors/{banFileMonitor.BanFileMonitorId}", Method.Patch, accessToken);
            request.AddJsonBody(banFileMonitor);

            var response = await ExecuteAsync(request);

            if (response.Content != null)
                return JsonConvert.DeserializeObject<BanFileMonitorDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task DeleteBanFileMonitor(string accessToken, Guid banFileMonitorId)
        {
            var request = CreateRequest($"repository/ban-file-monitors/{banFileMonitorId}", Method.Delete, accessToken);
            await ExecuteAsync(request);
        }
    }
}