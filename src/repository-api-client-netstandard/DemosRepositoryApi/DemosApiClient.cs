﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Net;
using System.Threading.Tasks;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard.DemosRepositoryApi
{
    public class DemosApiClient : BaseApiClient, IDemosApiClient
    {
        public DemosApiClient(ILogger<DemosApiClient> logger, IOptions<RepositoryApiClientOptions> options, IRepositoryApiTokenProvider repositoryApiTokenProvider) : base(logger, options, repositoryApiTokenProvider)
        {
        }

        public async Task<DemoDto?> CreateDemo(DemoDto demoDto, string fileName, string filePath)
        {
            var request = await CreateRequest("repository/demos", Method.Post);
            request.AddJsonBody(demoDto);
            request.AddFile(fileName, filePath);

            var response = await ExecuteAsync(request);

            if (response.Content != null)
                return JsonConvert.DeserializeObject<DemoDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task DeleteDemo(Guid demoId)
        {
            var request = await CreateRequest($"repository/demos/{demoId}", Method.Delete);
            await ExecuteAsync(request);
        }

        public async Task<DemoDto?> GetDemo(Guid demoId)
        {
            var request = await CreateRequest($"repository/demos/{demoId}", Method.Get);
            var response = await ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (response.Content != null)
                return JsonConvert.DeserializeObject<DemoDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<DemosSearchResponseDto> SearchDemos(string[]? gameTypes, string? userId, string? filterString, int skipEntries, int takeEntries, string? order)
        {
            var request = await CreateRequest("repository/demos", Method.Get);

            if (gameTypes != null)
                request.AddQueryParameter("gameTypes", string.Join(",", gameTypes));

            if (!string.IsNullOrWhiteSpace(userId))
                request.AddQueryParameter("userId", userId);

            if (!string.IsNullOrWhiteSpace(filterString))
                request.AddQueryParameter("filterString", filterString);

            request.AddQueryParameter("takeEntries", takeEntries.ToString());
            request.AddQueryParameter("skipEntries", skipEntries.ToString());

            if (!string.IsNullOrWhiteSpace(order))
                request.AddQueryParameter("order", order);

            var response = await ExecuteAsync(request);

            if (response.Content != null)
                return JsonConvert.DeserializeObject<DemosSearchResponseDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }
    }
}
