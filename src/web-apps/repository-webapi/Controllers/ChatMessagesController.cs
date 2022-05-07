using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryWebApi.Extensions;

namespace XtremeIdiots.Portal.RepositoryWebApi.Controllers;

[ApiController]
[Authorize(Roles = "ServiceAccount")]
public class ChatMessagesController : ControllerBase
{
    public ChatMessagesController(ILogger<ChatMessagesController> logger, PortalDbContext context)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public ILogger<ChatMessagesController> Logger { get; }
    public PortalDbContext Context { get; }

    [HttpGet]
    [Route("api/chat-messages/{chatMessageId}")]
    public async Task<IActionResult> GetChatMessage(Guid chatMessageId)
    {
        var chatLog = await Context.ChatLogs
            .Include(cl => cl.GameServerServer)
            .SingleOrDefaultAsync(cl => cl.ChatLogId == chatMessageId);

        if (chatLog == null)
            return new NotFoundResult();

        if (chatLog == null)
            return NotFound();

        return new OkObjectResult(chatLog.ToSearchEntryDto());
    }

    [HttpPost]
    [Route("api/chat-messages")]
    public async Task<IActionResult> CreateChatMessages()
    {
        var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

        List<ChatMessageDto> chatMessageDtos;
        try
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            chatMessageDtos = JsonConvert.DeserializeObject<List<ChatMessageDto>>(requestBody);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(ex);
        }

        if (chatMessageDtos == null) return new BadRequestResult();

        var chatLogs = chatMessageDtos.Select(chatMessageDto => new ChatLog()
        {
            PlayerPlayerId = chatMessageDto.PlayerId,
            GameServerServerId = chatMessageDto.GameServerId,
            Username = chatMessageDto.Username,
            ChatType = chatMessageDto.Type.ToChatTypeInt(),
            Message = chatMessageDto.Message,
            Timestamp = chatMessageDto.Timestamp
        });

        await Context.ChatLogs.AddRangeAsync(chatLogs);
        await Context.SaveChangesAsync();

        var result = chatLogs.Select(cl => cl.ToSearchEntryDto());

        return new OkObjectResult(result);
    }

    [HttpGet]
    [Route("api/chat-messages/search")]
    public async Task<IActionResult> SearchChatMessages(GameType? gameType, Guid? serverId, Guid? playerId, ChatMessageOrder? order, string? filterString, int skipEntries, int takeEntries)
    {
        if (gameType == null)
            gameType = GameType.Unknown;

        if (order == null)
            order = ChatMessageOrder.TimestampDesc;

        if (filterString == null)
            filterString = string.Empty;

        var query = Context.ChatLogs.AsQueryable();
        query = ApplySearchFilter(query, (GameType)gameType, serverId, playerId, string.Empty);
        var totalCount = await query.CountAsync();

        query = ApplySearchFilter(query, (GameType)gameType, serverId, playerId, filterString);
        var filteredCount = await query.CountAsync();

        query = ApplySearchOrderAndLimits(query, (ChatMessageOrder)order, skipEntries, takeEntries);
        var searchResults = await query.ToListAsync();

        var entries = searchResults.Select(cl => cl.ToSearchEntryDto()).ToList();

        var response = new ChatMessageSearchResponseDto
        {
            TotalRecords = totalCount,
            FilteredRecords = filteredCount,
            Entries = entries
        };

        return new OkObjectResult(response);
    }

    private IQueryable<ChatLog> ApplySearchFilter(IQueryable<ChatLog> chatLogs, GameType gameType, Guid? serverId, Guid? playerId, string filterString)
    {
        chatLogs = chatLogs.Include(cl => cl.GameServerServer).AsQueryable();

        if (gameType != GameType.Unknown) chatLogs = chatLogs.Where(cl => cl.GameServerServer.GameType == gameType.ToGameTypeInt()).AsQueryable();

        if (serverId != null) chatLogs = chatLogs.Where(cl => cl.GameServerServerId == serverId).AsQueryable();

        if (playerId != null) chatLogs = chatLogs.Where(cl => cl.PlayerPlayerId == playerId).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filterString))
            chatLogs = chatLogs.Where(m => m.Message.Contains(filterString)).AsQueryable();

        return chatLogs;
    }

    private IQueryable<ChatLog> ApplySearchOrderAndLimits(IQueryable<ChatLog> chatLogs, ChatMessageOrder order, int skipEntries, int takeEntries)
    {
        switch (order)
        {
            case ChatMessageOrder.TimestampAsc:
                chatLogs = chatLogs.OrderBy(cl => cl.Timestamp).AsQueryable();
                break;
            case ChatMessageOrder.TimestampDesc:
                chatLogs = chatLogs.OrderByDescending(cl => cl.Timestamp).AsQueryable();
                break;
        }

        chatLogs = chatLogs.Skip(skipEntries).AsQueryable();

        if (takeEntries != 0) chatLogs = chatLogs.Take(takeEntries).AsQueryable();

        return chatLogs;
    }
}