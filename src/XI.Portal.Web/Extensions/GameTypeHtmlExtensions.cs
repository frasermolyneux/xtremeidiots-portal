using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using XI.Portal.Data.Legacy.CommonTypes;

namespace XI.Portal.Web.Extensions
{
    public static class GameTypeHtmlExtensions
    {
        public static HtmlString GameTypeIcon(this IHtmlHelper html, GameType gameType)
        {
            return new HtmlString(
                $"<img src=\"/images/game-icons/{gameType}.png\" alt=\"{gameType}\" width=\"16\" height=\"16\" />");
        }
    }
}