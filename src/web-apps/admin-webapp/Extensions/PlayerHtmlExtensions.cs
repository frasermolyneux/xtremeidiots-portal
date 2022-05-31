using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace XtremeIdiots.Portal.AdminWebApp.Extensions
{
    public static class PlayerHtmlExtensions
    {
        public static string PlayerName(this IHtmlHelper html, string playerName)
        {
            var toRemove = new List<string> { "^1", "^2", "^3", "^4", "^5", "^6", "^7", "^8", "^9" };
            foreach (var val in toRemove) playerName = playerName.Replace(val, "");

            return playerName;
        }

        public static HtmlString GuidLink(this IHtmlHelper html, string guid, string gameType)
        {
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