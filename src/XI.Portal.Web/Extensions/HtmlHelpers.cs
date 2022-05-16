using Microsoft.AspNetCore.Mvc.Rendering;

namespace XI.Portal.Web.Extensions
{
    public static class HtmlHelpers
    {
        public static string IsSelected(this IHtmlHelper html, string controller = null, string action = null, string id = null, string cssClass = null)
        {
            if (string.IsNullOrEmpty(cssClass))
                cssClass = "active";

            var currentAction = (string)html.ViewContext.RouteData.Values["action"];
            var currentController = (string)html.ViewContext.RouteData.Values["controller"];
            var currentId = (string)html.ViewContext.RouteData.Values["id"];

            if (string.IsNullOrEmpty(controller))
                controller = currentController;

            if (string.IsNullOrEmpty(action))
                action = currentAction;

            if (string.IsNullOrEmpty(id))
                id = currentId;

            return controller == currentController && action == currentAction && id == currentId ? cssClass : string.Empty;
        }

        public static string PageClass(this IHtmlHelper htmlHelper)
        {
            var currentAction = (string)htmlHelper.ViewContext.RouteData.Values["action"];
            return currentAction;
        }
    }
}