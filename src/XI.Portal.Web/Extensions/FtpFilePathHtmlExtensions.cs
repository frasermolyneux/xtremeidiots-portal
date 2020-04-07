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
    }
}