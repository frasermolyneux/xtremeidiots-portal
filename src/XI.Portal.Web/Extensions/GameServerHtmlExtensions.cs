using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using XI.CommonTypes;

namespace XI.Portal.Web.Extensions
{
    public static class GameServerHtmlExtensions
    {
        public static HtmlString ServerHostAndPort(this IHtmlHelper html, string hostname, int port)
        {
            if (string.IsNullOrWhiteSpace(hostname) && port == 0) return new HtmlString("");

            if (!string.IsNullOrWhiteSpace(hostname) && port == 0) return new HtmlString(hostname);

            return new HtmlString($"{hostname}:{port}");
        }

        public static HtmlString ServerName(this IHtmlHelper html, string title, string liveTitle)
        {
            var toRemove = new List<string> {"^1", "^2", "^3", "^4", "^5", "^6", "^7", "^8", "^9"};

            if (string.IsNullOrWhiteSpace(liveTitle)) return new HtmlString(title);

            var serverName = toRemove.Aggregate(liveTitle, (current, val) => current.Replace(val, ""));
            return new HtmlString(serverName);
        }

        public static HtmlString GameTrackerIcon(this IHtmlHelper html, string hostname, int port)
        {
            var link = $"https://www.gametracker.com/server_info/{hostname}:{port}";
            return new HtmlString(
                $"<a style=\"margin:5px\" href=\"{link}\" target=\"_blank\"><img src=\"/images/service-icons/gametracker.png\" alt=\"gametracker\"/></a>");
        }

        public static HtmlString HlswIcon(this IHtmlHelper html, GameType gameType, string hostname, int port)
        {
            var link = "";
            switch (gameType)
            {
                case GameType.CallOfDuty2:
                    link = $"hlsw://{hostname}:{port}?Game=CoD2";
                    break;
                case GameType.CallOfDuty4:
                    link = $"hlsw://{hostname}:{port}?Game=CoD4";
                    break;
                case GameType.CallOfDuty5:
                    link = $"hlsw://{hostname}:{port}?Game=CoDWW";
                    break;
                default:
                    return new HtmlString("");
            }

            return new HtmlString(
                $"<a style=\"margin:5px\" href=\"{link}\"><img src=\"/images/service-icons/hlsw.png\" alt=\"hlsw\"/></a>");
        }

        public static HtmlString SteamIcon(this IHtmlHelper html, GameType gameType, string hostname, int port)
        {
            var link = $"steam://connect/{hostname}:{port}";
            return new HtmlString(
                $"<a style=\"margin:5px\" href=\"{link}\"><img src=\"/images/service-icons/steam.png\" alt=\"steam\"/></a>");
        }
    }
}