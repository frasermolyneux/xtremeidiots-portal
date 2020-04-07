using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

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
    }
}