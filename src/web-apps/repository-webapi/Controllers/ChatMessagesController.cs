using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

using System.Net;

using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.ChatMessages;
using XtremeIdiots.Portal.RepositoryWebApi.Extensions;

namespace XtremeIdiots.Portal.RepositoryWebApi.Controllers;

[ApiController]
[Authorize(Roles = "ServiceAccount")]
public class ChatMessagesController : ControllerBase, IChatMessagesApi
{
    private readonly PortalDbContext context;
    private readonly IMapper mapper;

    public ChatMessagesController(
        PortalDbContext context,
        IMapper mapper)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    [HttpGet]
    [Route("repository/chat-messages/{chatMessageId}")]
    public async Task<IActionResult> GetChatMessage(Guid chatMessageId)
    {
        var response = await ((IChatMessagesApi)this).GetChatMessage(chatMessageId);

        return response.ToHttpResult();
    }

    async Task<ApiResponseDto<ChatMessageDto>> IChatMessagesApi.GetChatMessage(Guid chatMessageId)
    {
        var chatLog = await context.ChatMessages
            .Include(cl => cl.GameServer)
            .Include(cl => cl.Player)
            .SingleOrDefaultAsync(cl => cl.ChatMessageId == chatMessageId);

        if (chatLog == null)
            return new ApiResponseDto<ChatMessageDto>(HttpStatusCode.NotFound);

        var result = mapper.Map<ChatMessageDto>(chatLog);

        return new ApiResponseDto<ChatMessageDto>(HttpStatusCode.OK, result);
    }

    [HttpGet]
    [Route("repository/chat-messages")]
    public async Task<IActionResult> GetChatMessages(GameType? gameType, Guid? gameServerId, Guid? playerId, string? filterString, int? skipEntries, int? takeEntries, ChatMessageOrder? order)
    {
        if (!skipEntries.HasValue)
            skipEntries = 0;

        if (!takeEntries.HasValue)
            takeEntries = 20;

        var response = await ((IChatMessagesApi)this).GetChatMessages(gameType, gameServerId, playerId, filterString, skipEntries.Value, takeEntries.Value, order);

        return response.ToHttpResult();
    }

    async Task<ApiResponseDto<ChatMessagesCollectionDto>> IChatMessagesApi.GetChatMessages(GameType? gameType, Guid? gameServerId, Guid? playerId, string? filterString, int skipEntries, int takeEntries, ChatMessageOrder? order)
    {
        var query = context.ChatMessages.Include(cl => cl.GameServer).Include(cl => cl.Player).AsQueryable();
        query = ApplyFilter(query, gameType, gameServerId, playerId, string.Empty);
        var totalCount = await query.CountAsync();

        query = ApplyFilter(query, gameType, gameServerId, playerId, filterString);
        var filteredCount = await query.CountAsync();

        query = ApplyOrderAndLimits(query, skipEntries, takeEntries, order);
        var results = await query.ToListAsync();

        var entries = results.Select(cm => mapper.Map<ChatMessageDto>(cm)).ToList();

        var result = new ChatMessagesCollectionDto
        {
            TotalRecords = totalCount,
            FilteredRecords = filteredCount,
            Entries = entries
        };

        return new ApiResponseDto<ChatMessagesCollectionDto>(HttpStatusCode.OK, result);
    }

    Task<ApiResponseDto> IChatMessagesApi.CreateChatMessage(CreateChatMessageDto createChatMessageDto)
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    [Route("repository/chat-messages")]
    public async Task<IActionResult> CreateChatMessages()
    {
        var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

        List<CreateChatMessageDto>? createChatMessageDtos;
        try
        {
            createChatMessageDtos = JsonConvert.DeserializeObject<List<CreateChatMessageDto>>(requestBody);
        }
        catch
        {
            return new ApiResponseDto(HttpStatusCode.BadRequest, "Could not deserialize request body").ToHttpResult();
        }

        if (createChatMessageDtos == null || !createChatMessageDtos.Any())
            return new ApiResponseDto(HttpStatusCode.BadRequest, "Request body was null or did not contain any entries").ToHttpResult();

        var response = await ((IChatMessagesApi)this).CreateChatMessages(createChatMessageDtos);

        return response.ToHttpResult();
    }

    async Task<ApiResponseDto> IChatMessagesApi.CreateChatMessages(List<CreateChatMessageDto> createChatMessageDtos)
    {
        var chatLogs = createChatMessageDtos.Select(cm => mapper.Map<ChatMessage>(cm)).ToList();

        await context.ChatMessages.AddRangeAsync(chatLogs);
        await context.SaveChangesAsync();

        return new ApiResponseDto(HttpStatusCode.OK);
    }

    private IQueryable<ChatMessage> ApplyFilter(IQueryable<ChatMessage> query, GameType? gameType, Guid? gameServerId, Guid? playerId, string? filterString)
    {
        if (gameType.HasValue)
            query = query.Where(cl => cl.GameServer.GameType == gameType.Value.ToGameTypeInt()).AsQueryable();

        if (gameServerId.HasValue)
            query = query.Where(cl => cl.GameServerId == gameServerId).AsQueryable();

        if (playerId.HasValue)
            query = query.Where(cl => cl.PlayerId == playerId).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filterString))
            query = query.Where(m => m.Message.Contains(filterString)).AsQueryable();

        return query;
    }

    private IQueryable<ChatMessage> ApplyOrderAndLimits(IQueryable<ChatMessage> query, int skipEntries, int takeEntries, ChatMessageOrder? order)
    {
        switch (order)
        {
            case ChatMessageOrder.TimestampAsc:
                query = query.OrderBy(cl => cl.Timestamp).AsQueryable();
                break;
            case ChatMessageOrder.TimestampDesc:
                query = query.OrderByDescending(cl => cl.Timestamp).AsQueryable();
                break;
        }

        query = query.Skip(skipEntries).AsQueryable();
        query = query.Take(takeEntries).AsQueryable();

        return query;
    }
}