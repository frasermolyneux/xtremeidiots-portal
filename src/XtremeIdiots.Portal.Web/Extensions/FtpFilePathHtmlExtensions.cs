using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace XtremeIdiots.Portal.Web.Extensions;

public static class FtpFilePathHtmlExtensions
{
    public static HtmlString MonitorFtpPath(this IHtmlHelper html, string? ftpHostName, int ftpPort, string? filePath)
    {
        return string.IsNullOrWhiteSpace(ftpHostName) || string.IsNullOrWhiteSpace(filePath)
            ? new HtmlString(string.Empty)
            : new HtmlString(
            $"ftp://{ftpHostName}:{ftpPort}{filePath}");
    }

    public static HtmlString MonitorFtpPath(this IHtmlHelper html, string? ftpHostName, int ftpPort, string? filePath, string? ftpUsername, string? ftpPassword)
    {
        if (string.IsNullOrWhiteSpace(ftpHostName) || string.IsNullOrWhiteSpace(filePath))
            return new HtmlString(string.Empty);

        return string.IsNullOrWhiteSpace(ftpUsername) || string.IsNullOrWhiteSpace(ftpPassword)
            ? MonitorFtpPath(html, ftpHostName, ftpPort, filePath)
            : new HtmlString($"<a target=\"_blank\" href=\"ftp://{ftpUsername}:{ftpPassword}@{ftpHostName}:{ftpPort}{filePath}\">ftp://{ftpHostName}:{ftpPort}{filePath}</a>");
    }
}