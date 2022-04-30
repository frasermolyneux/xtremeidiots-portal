﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using XI.CommonTypes;
using XI.Portal.Data.Legacy;
using XI.Portal.Data.Legacy.Models;
using XtremeIdiots.Portal.CommonLib.Models;

namespace XtremeIdiots.Portal.RepositoryWebApi.Controllers;

[ApiController]
[Authorize(Roles = "ServiceAccount")]
public class ChatMessagesController : ControllerBase
{
    public ChatMessagesController(ILogger<ChatMessagesController> logger, LegacyPortalContext context)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public ILogger<ChatMessagesController> Logger { get; }
    public LegacyPortalContext Context { get; }

    [HttpGet]
    [Route("api/chat-messages/{chatMessageId}")]
    public async Task<IActionResult> GetPlayer(Guid chatMessageId)
    {
        var chatMessage = await Context.ChatLogs.Include(cl => cl.GameServerServer).SingleOrDefaultAsync(cl => cl.ChatLogId == chatMessageId);

        if (chatMessage == null) return new NotFoundResult();

        var chatMessageSearchEntryDto = new ChatMessageSearchEntryDto
        {
            ChatLogId = chatMessage.ChatLogId,
            PlayerId = chatMessage.PlayerPlayerId,
            ServerId = chatMessage.GameServerServerId,
            ServerName = chatMessage.GameServerServer.Title,
            GameType = chatMessage.GameServerServer.GameType.ToString(),
            Timestamp = chatMessage.Timestamp,
            Username = chatMessage.Username,
            ChatType = chatMessage.ChatType.ToString(),
            Message = chatMessage.Message
        };

        return new OkObjectResult(chatMessageSearchEntryDto);
    }

    [HttpPost]
    [Route("api/chat-messages")]
    public async Task<IActionResult> CreateChatMessage()
    {
        var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

        List<ChatMessageApiDto> chatMessages;
        try
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            chatMessages = JsonConvert.DeserializeObject<List<ChatMessageApiDto>>(requestBody);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(ex);
        }

        if (chatMessages == null) return new BadRequestResult();

        foreach (var chatMessage in chatMessages)
        {
            Context.ChatLogs.Add(new ChatLogs()
            {
                PlayerPlayerId = chatMessage.PlayerId,
                GameServerServerId = Guid.Parse(chatMessage.GameServerId),
                Username = chatMessage.Username,
                ChatType = chatMessage.Type == "Team" ? XI.CommonTypes.ChatType.Team : XI.CommonTypes.ChatType.All,
                Message = chatMessage.Message,
                Timestamp = chatMessage.Timestamp
            });
        }

        await Context.SaveChangesAsync();

        return new OkObjectResult(chatMessages);
    }

    [HttpGet]
    [Route("api/chat-messages/search")]
    public async Task<IActionResult> SearchChatMessages(string? gameType, Guid? serverId, Guid? playerId, string? order, string? filterString, int skipEntries, int takeEntries)
    {
        GameType legacyGameType;
        switch (gameType)
        {
            case "CallOfDuty2":
                legacyGameType = GameType.CallOfDuty2;
                break;
            case "CallOfDuty4":
                legacyGameType = GameType.CallOfDuty4;
                break;
            case "CallOfDuty5":
                legacyGameType = GameType.CallOfDuty5;
                break;
            default:
                legacyGameType = GameType.Unknown;
                break;
        }

        if (string.IsNullOrWhiteSpace(order))
            order = "TimestampDesc";

        if (filterString == null)
            filterString = string.Empty;

        var query = Context.ChatLogs.AsQueryable();
        query = ApplySearchFilter(query, legacyGameType, serverId, playerId, string.Empty);
        var totalCount = await query.CountAsync();

        query = ApplySearchFilter(query, legacyGameType, serverId, playerId, filterString);
        var filteredCount = await query.CountAsync();

        query = ApplySearchOrderAndLimits(query, order, skipEntries, takeEntries);
        var searchResults = await query.ToListAsync();

        var entries = searchResults.Select(cl => new ChatMessageSearchEntryDto()
        {
            ChatLogId = cl.ChatLogId,
            PlayerId = cl.PlayerPlayerId,
            ServerId = cl.GameServerServerId,
            ServerName = cl.GameServerServer.Title,
            GameType = cl.GameServerServer.GameType.ToString(),
            Timestamp = cl.Timestamp,
            Username = cl.Username,
            ChatType = cl.ChatType.ToString(),
            Message = cl.Message
        }).ToList();

        var response = new ChatMessageSearchResponseDto
        {
            TotalRecords = totalCount,
            FilteredRecords = filteredCount,
            Entries = entries
        };

        Logger.LogInformation($"SearchChatMessages :: {JsonConvert.SerializeObject(response)}");

        return new OkObjectResult(response);
    }

    private IQueryable<ChatLogs> ApplySearchFilter(IQueryable<ChatLogs> chatLogs, GameType gameType, Guid? serverId, Guid? playerId, string filterString)
    {
        chatLogs = chatLogs.Include(cl => cl.GameServerServer).AsQueryable();

        if (gameType != GameType.Unknown) chatLogs = chatLogs.Where(m => m.GameServerServer.GameType == gameType).AsQueryable();

        if (serverId != null) chatLogs = chatLogs.Where(m => m.GameServerServerId == serverId).AsQueryable();

        if (playerId != null) chatLogs = chatLogs.Where(m => m.PlayerPlayerId == playerId).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filterString))
            chatLogs = chatLogs.Where(m => m.Message.Contains(filterString)).AsQueryable();

        return chatLogs;
    }

    private IQueryable<ChatLogs> ApplySearchOrderAndLimits(IQueryable<ChatLogs> chatLogs, string order, int skipEntries, int takeEntries)
    {
        switch (order)
        {
            case "TimestampAsc":
                chatLogs = chatLogs.OrderBy(cl => cl.Timestamp).AsQueryable();
                break;
            case "TimestampDesc":
                chatLogs = chatLogs.OrderByDescending(cl => cl.Timestamp).AsQueryable();
                break;
        }

        chatLogs = chatLogs.Skip(skipEntries).AsQueryable();

        if (takeEntries != 0) chatLogs = chatLogs.Take(takeEntries).AsQueryable();

        return chatLogs;
    }
}