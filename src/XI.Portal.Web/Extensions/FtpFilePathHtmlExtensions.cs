using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace XI.Portal.Web.Extensions
{
    public static class FtpFilePathHtmlExtensions
    {
        public static HtmlString MonitorFtpPath(this IHtmlHelper html, string ftpHostName, string filePath)
        {
            return new HtmlString(
                $"ftp://{ftpHostName}{filePath}");
        }

        public static HtmlString MonitorFtpPath(this IHtmlHelper html, string ftpHostName, string filePath, string ftpUsername, string ftpPassword)
        {
            return new HtmlString($"<a target=\"_blank\" href=\"ftp://{ftpUsername}:{ftpPassword}@{ftpHostName}{filePath}\">ftp://{ftpHostName}{filePath}</a>");
        }
    }
}
