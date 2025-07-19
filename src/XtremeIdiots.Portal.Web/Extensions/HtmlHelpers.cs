using Microsoft.AspNetCore.Mvc.Rendering;

namespace XtremeIdiots.Portal.Web.Extensions
{
    public static class HtmlHelpers
    {
        /// <summary>
        /// Determines if a menu item should be highlighted as selected based on the current route.
        /// </summary>
        /// <param name="html">The HTML helper instance</param>
        /// <param name="controller">Controller name to match</param>
        /// <param name="action">Action name to match (if null, only controller is checked)</param>
        /// <param name="id">Route ID to match (if provided)</param>
        /// <param name="cssClass">CSS class to apply if selected (default: "active")</param>
        /// <returns>The CSS class if the item is selected, empty string otherwise</returns>
        public static string IsSelected(this IHtmlHelper html, string? controller = null, string? action = null, string? id = null, string? cssClass = null)
        {
            if (string.IsNullOrEmpty(cssClass))
                cssClass = "active";

            var currentAction = html.ViewContext.RouteData.Values["action"]?.ToString() ?? string.Empty;
            var currentController = html.ViewContext.RouteData.Values["controller"]?.ToString() ?? string.Empty;
            var currentId = html.ViewContext.RouteData.Values["id"]?.ToString();

            // If no controller specified, use current controller
            if (string.IsNullOrEmpty(controller))
                controller = currentController;

            // For parent menu items (with no action specified), only highlight if it's an exact controller match
            // AND we're at either the Index action or there's no explicit submenu item selected
            if (action is null)
            {
                if (controller == currentController)
                {
                    // For parent items, only highlight when we're at the Index action
                    // This prevents the parent from being highlighted when a child item is selected
                    if (currentAction == "Index")
                        return cssClass;

                    // For non-Index actions, don't highlight the parent
                    return string.Empty;
                }
            }
            // For specific menu items with explicit actions
            else if (controller == currentController)
            {
                // Exact match on action required
                if (action == currentAction)
                {
                    // If ID parameter is provided, it must match too
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