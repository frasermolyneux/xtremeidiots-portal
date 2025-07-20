using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace XtremeIdiots.Portal.Web.Extensions;

public static class PlayerHtmlExtensions
{
    public static string PlayerName(this IHtmlHelper html, string? playerName)
    {
        if (string.IsNullOrWhiteSpace(playerName))
            return string.Empty;

        var toRemove = new List<string> { "^1", "^2", "^3", "^4", "^5", "^6", "^7", "^8", "^9" };
        foreach (var val in toRemove)
            playerName = playerName.Replace(val, "");

        return playerName;
    }

    public static HtmlString GuidLink(this IHtmlHelper html, string? playerGuid, string? gameType)
    {
        if (string.IsNullOrWhiteSpace(playerGuid) || string.IsNullOrWhiteSpace(gameType))
            return new HtmlString(playerGuid ?? string.Empty);

        switch (gameType)
        {
            case "CallOfDuty4":
                var link = $"https://www.pbbans.com/mbi.php?action=12&guid={playerGuid}";
                return new HtmlString(
                    $"<a style=\"margin:5px\" href=\"{link}\" target=\"_blank\">{playerGuid}</a>");
            default:
                return new HtmlString(playerGuid);
        }
    }
}