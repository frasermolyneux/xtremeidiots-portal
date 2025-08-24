using Microsoft.AspNetCore.Razor.TagHelpers;

namespace XtremeIdiots.Portal.Web.Helpers;

/// <summary>
/// Renders a <time> element with a relative "time ago" description and the absolute datetime in attributes.
/// Usage: <time time-ago utc="@Model.Created" show-absolute="true"></time>
/// </summary>
[HtmlTargetElement("time", Attributes = AttributeName)]
public class TimeAgoTagHelper : TagHelper
{
    private const string UtcAttributeName = "utc";
    private const string ShowAbsoluteAttributeName = "show-absolute";
    internal const string AttributeName = "time-ago"; // marker attribute

    [HtmlAttributeName(UtcAttributeName)]
    public DateTime Utc { get; set; }

    [HtmlAttributeName(ShowAbsoluteAttributeName)]
    public bool ShowAbsolute { get; set; } = true;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        // Always render <time>
        output.TagName = "time";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("datetime", Utc.ToString("o"));
        var rel = BuildRelative(Utc);
        var absolute = Utc.ToString("yyyy-MM-dd HH:mm");
        output.Content.SetHtmlContent(ShowAbsolute ? $"{rel} ({absolute} UTC)" : rel);
        output.Attributes.RemoveAll(AttributeName);
        output.Attributes.RemoveAll(UtcAttributeName);
        output.Attributes.RemoveAll(ShowAbsoluteAttributeName);
    }

    private static string BuildRelative(DateTime dateTimeUtc)
    {
        var now = DateTime.UtcNow;
        var future = dateTimeUtc > now;
        var span = future ? dateTimeUtc - now : now - dateTimeUtc;
        var seconds = span.TotalSeconds;
        var minutes = span.TotalMinutes;
        var hours = span.TotalHours;
        var days = span.TotalDays;
        if (seconds < 45) return future ? "in a few seconds" : "just now";
        if (seconds < 90) return future ? "in a minute" : "a minute ago";
        if (minutes < 45) return future ? $"in {Math.Round(minutes)} minutes" : $"{Math.Round(minutes)} minutes ago";
        if (minutes < 90) return future ? "in an hour" : "an hour ago";
        if (hours < 24) return future ? $"in {Math.Round(hours)} hours" : $"{Math.Round(hours)} hours ago";
        if (hours < 42) return future ? "in a day" : "a day ago";
        if (days < 30) return future ? $"in {Math.Round(days)} days" : $"{Math.Round(days)} days ago";
        if (days < 45) return future ? "in a month" : "a month ago";
        if (days < 365) return future ? $"in {Math.Round(days / 30)} months" : $"{Math.Round(days / 30)} months ago";
        if (days < 545) return future ? "in a year" : "a year ago";
        return future ? $"in {Math.Round(days / 365)} years" : $"{Math.Round(days / 365)} years ago";
    }
}
