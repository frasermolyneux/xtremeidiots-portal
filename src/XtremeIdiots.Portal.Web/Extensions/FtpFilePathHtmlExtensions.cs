using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace XtremeIdiots.Portal.Web.Extensions
{
    public static class FtpFilePathHtmlExtensions
    {
        public static HtmlString MonitorFtpPath(this IHtmlHelper html, string? ftpHostName, int ftpPort, string? filePath)
        {
            if (string.IsNullOrWhiteSpace(ftpHostName) || string.IsNullOrWhiteSpace(filePath))
                return new HtmlString(string.Empty);

            return new HtmlString(
                $"ftp://{ftpHostName}:{ftpPort}{filePath}");
        }

        public static HtmlString MonitorFtpPath(this IHtmlHelper html, string? ftpHostName, int ftpPort, string? filePath, string? ftpUsername, string? ftpPassword)
        {
            if (string.IsNullOrWhiteSpace(ftpHostName) || string.IsNullOrWhiteSpace(filePath))
                return new HtmlString(string.Empty);

            if (string.IsNullOrWhiteSpace(ftpUsername) || string.IsNullOrWhiteSpace(ftpPassword))
                return MonitorFtpPath(html, ftpHostName, ftpPort, filePath);

            return new HtmlString($"<a target=\"_blank\" href=\"ftp://{ftpUsername}:{ftpPassword}@{ftpHostName}:{ftpPort}{filePath}\">ftp://{ftpHostName}:{ftpPort}{filePath}</a>");
        }
    }
}