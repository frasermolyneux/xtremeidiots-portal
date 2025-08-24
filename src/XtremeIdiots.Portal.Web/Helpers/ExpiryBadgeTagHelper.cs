using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Security.Claims;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;

namespace XtremeIdiots.Portal.Web.Helpers;

/// <summary>
/// Renders an expiry date with Active / Expired badge inside a <span>.
/// Usage: <expiry-badge expires-utc="@Model.Expires" user="@User" />
/// </summary>
[HtmlTargetElement("expiry-badge")]
public class ExpiryBadgeTagHelper : TagHelper
{
    [HtmlAttributeName("expires-utc")] public DateTime? ExpiresUtc { get; set; }
    [HtmlAttributeName("user")] public ClaimsPrincipal? User { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        if (ExpiresUtc is null)
        {
            output.Content.SetHtmlContent("<span title=\"No expiry\">Never</span>");
            return;
        }

        var now = DateTime.UtcNow;
        var expired = ExpiresUtc.Value <= now;
        var displayDate = ExpiresUtc.Value;
        // timezone via claim (same logic as existing helper)
        var tzClaim = User?.Claims.SingleOrDefault(c => c.Type == UserProfileClaimType.TimeZone);
        if (tzClaim != null)
        {
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById(tzClaim.Value);
                displayDate = TimeZoneInfo.ConvertTime(displayDate, tz);
            }
            catch { }
        }

        var dateStr = displayDate.ToString("D", System.Globalization.CultureInfo.CurrentUICulture);
        var badgeClass = expired ? "text-bg-danger" : "text-bg-success";
        var badgeText = expired ? "Expired" : "Active";
        var title = expired ? $"Expired on {dateStr}" : $"Expires on {dateStr}";
        output.Attributes.SetAttribute("title", title);
        output.Content.SetHtmlContent($"{dateStr} <span class='badge {badgeClass} ms-1'>{badgeText}</span>");
    }
}
