using Microsoft.AspNetCore.Mvc.Rendering;

namespace XtremeIdiots.Portal.Web.Extensions
{
    public static class HtmlHelpers
    {

        public static string IsSelected(this IHtmlHelper html, string? controller = null, string? action = null, string? id = null, string? cssClass = null)
        {
            if (string.IsNullOrEmpty(cssClass))
                cssClass = "active";

            var currentAction = html.ViewContext.RouteData.Values["action"]?.ToString() ?? string.Empty;
            var currentController = html.ViewContext.RouteData.Values["controller"]?.ToString() ?? string.Empty;
            var currentId = html.ViewContext.RouteData.Values["id"]?.ToString();

            if (string.IsNullOrEmpty(controller))
                controller = currentController;

            if (action is null)
            {
                if (controller == currentController)
                {

                    if (currentAction == "Index")
                        return cssClass;

                    return string.Empty;
                }
            }

            else if (controller == currentController)
            {

                if (action == currentAction)
                {

                    if (id != null)
                        return id == currentId ? cssClass : string.Empty;

                    return cssClass;
                }
            }

            return string.Empty;
        }

        public static string PageClass(this IHtmlHelper htmlHelper)
        {
            var currentAction = htmlHelper.ViewContext.RouteData.Values["action"]?.ToString() ?? string.Empty;
            return currentAction;
        }
    }
}