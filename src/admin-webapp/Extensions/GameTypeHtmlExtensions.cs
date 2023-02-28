using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.AdminWebApp.Extensions
{
    public static class GameTypeHtmlExtensions
    {
        public static HtmlString GameTypeIcon(this IHtmlHelper html, GameType gameType)
        {
            return new HtmlString(
                $"<img src=\"/images/game-icons/{gameType}.png\" alt=\"{gameType}\" width=\"16\" height=\"16\" />");
        }

        public static HtmlString GameTypeIconExternal(this IHtmlHelper html, GameType gameType)
        {
            return new HtmlString($"<img src=\"https://portal.xtremeidiots.com/images/game-icons/{gameType}.png\" alt=\"{gameType}\" width=\"16\" height=\"16\" />");
        }
    }
}