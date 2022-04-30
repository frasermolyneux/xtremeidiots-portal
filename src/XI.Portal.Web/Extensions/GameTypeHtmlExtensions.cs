using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace XI.Portal.Web.Extensions
{
    public static class GameTypeHtmlExtensions
    {
        public static HtmlString GameTypeIcon(this IHtmlHelper html, string gameType)
        {
            return new HtmlString(
                $"<img src=\"/images/game-icons/{gameType}.png\" alt=\"{gameType}\" width=\"16\" height=\"16\" />");
        }

        public static HtmlString GameTypeIconExternal(this IHtmlHelper html, string gameType)
        {
            return new HtmlString($"<img src=\"https://portal.xtremeidiots.com/images/game-icons/{gameType}.png\" alt=\"{gameType}\" width=\"16\" height=\"16\" />");
        }
    }
}