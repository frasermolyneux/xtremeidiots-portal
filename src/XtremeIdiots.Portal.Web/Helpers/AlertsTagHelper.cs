using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Newtonsoft.Json;
using XtremeIdiots.Portal.Web.Models;

namespace XtremeIdiots.Portal.Web.Helpers;

public class AlertsTagHelper : TagHelper
{
    private const string AlertKey = "Alerts";

    [ViewContext]
    public required ViewContext ViewContext { get; set; }

    protected ITempDataDictionary TempData => ViewContext.TempData;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        // We emit a lightweight container div with a data attribute containing serialized alerts.
        // site.js will detect this and raise standardized toastr notifications (with graceful fallback
        // to inline Bootstrap alerts if toastr is unavailable).
        output.TagName = "div";
        output.Attributes.SetAttribute("id", "server-alerts-data");

        if (TempData[AlertKey] is null)
            TempData[AlertKey] = JsonConvert.SerializeObject(new HashSet<Alert>());

        var alertsJson = TempData[AlertKey]?.ToString() ?? "[]";
        var alerts = JsonConvert.DeserializeObject<ICollection<Alert>>(alertsJson) ?? [];

        // Store as data attribute for client script to process.
        var safeJson = JsonConvert.SerializeObject(alerts); // ensures proper escaping
        output.Attributes.SetAttribute("data-alerts", safeJson);

        // Provide a <noscript> fallback with traditional Bootstrap alerts for users without JS.
        if (alerts.Count > 0)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("<noscript>");
            foreach (var alert in alerts)
            {
                sb.Append($"<div class='alert {alert.Type}' role='alert'>{System.Net.WebUtility.HtmlEncode(alert.Message)}</div>");
            }

            sb.Append("</noscript>");
            output.Content.SetHtmlContent(sb.ToString());
        }
        else
        {
            output.Content.SetHtmlContent(string.Empty);
        }
    }
}