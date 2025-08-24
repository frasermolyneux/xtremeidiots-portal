using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Security.Claims;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;

namespace XtremeIdiots.Portal.Web.Helpers;

/// <summary>
/// Renders a <time> element converted into the user's timezone based on the TimeZone claim.
/// Usage: <time user-time utc="@model.Date" /> (user resolved from ViewContext)
/// </summary>
[HtmlTargetElement("time", Attributes = AttributeName)]
public class UserTimeTagHelper : TagHelper
{
    internal const string AttributeName = "user-time";
    private const string UtcAttributeName = "utc";
    private const string FormatAttributeName = "format";

    [HtmlAttributeName(UtcAttributeName)] public DateTime Utc { get; set; }
    [HtmlAttributeName(FormatAttributeName)] public string? Format { get; set; }

    [ViewContext] public ViewContext? ViewContext { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "time";
        output.TagMode = TagMode.StartTagAndEndTag;
        var user = ViewContext?.HttpContext?.User;
        var display = ConvertToUserTime(user, Utc);
        var text = Format is not null ? display.ToString(Format) : display.ToString();
        output.Attributes.SetAttribute("datetime", Utc.ToString("o"));
        output.Content.SetContent(text);
        output.Attributes.RemoveAll(AttributeName);
        output.Attributes.RemoveAll(UtcAttributeName);
        output.Attributes.RemoveAll(FormatAttributeName);
    }

    private static DateTime ConvertToUserTime(ClaimsPrincipal? user, DateTime utc)
    {
        if (user == null)
            return utc;
        var timezoneClaim = user.Claims.SingleOrDefault(c => c.Type == UserProfileClaimType.TimeZone);
        if (timezoneClaim is null)
            return utc;
        try
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timezoneClaim.Value);
            return TimeZoneInfo.ConvertTime(utc, tz);
        }
        catch
        {
            return utc;
        }
    }
}
