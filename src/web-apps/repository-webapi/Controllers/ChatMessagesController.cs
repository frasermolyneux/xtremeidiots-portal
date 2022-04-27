using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using XI.Portal.Data.Legacy;
using XI.Portal.Data.Legacy.Models;
using XtremeIdiots.Portal.CommonLib.Models;

namespace XtremeIdiots.Portal.RepositoryWebApi.Controllers;

[ApiController]
[Authorize(Roles = "ServiceAccount")]
public class ChatMessagesController : ControllerBase
{
    public ChatMessagesController(LegacyPortalContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public LegacyPortalContext Context { get; }

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
}