using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace XtremeIdiots.Portal.Web.Extensions
{
    public static class PlayerHtmlExtensions
    {
        public static string PlayerName(this IHtmlHelper html, string? playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName))
                return string.Empty;

            var toRemove = new List<string> { "^1", "^2", "^3", "^4", "^5", "^6", "^7", "^8", "^9" };
            foreach (var val in toRemove) playerName = playerName.Replace(val, "");

            return playerName;
        }

        public static HtmlString GuidLink(this IHtmlHelper html, string? guid, string? gameType)
        {
            if (string.IsNullOrWhiteSpace(guid) || string.IsNullOrWhiteSpace(gameType))
                return new HtmlString(guid ?? string.Empty);

            switch (gameType)
            {
                case "CallOfDuty4":
                    var link = $"https://www.pbbans.com/mbi.php?action=12&guid={guid}";
                    return new HtmlString(
                        $"<a style=\"margin:5px\" href=\"{link}\" target=\"_blank\">{guid}</a>");
                default:
                    return new HtmlString(guid);
            }
        }
    }
}