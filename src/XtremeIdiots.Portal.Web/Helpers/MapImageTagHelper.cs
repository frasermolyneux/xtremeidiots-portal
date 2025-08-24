using Microsoft.AspNetCore.Razor.TagHelpers;

namespace XtremeIdiots.Portal.Web.Helpers;

[HtmlTargetElement("map-image")]
public class MapImageTagHelper : TagHelper
{
    [HtmlAttributeName("uri")] public string? Uri { get; set; }
    [HtmlAttributeName("game-type")] public string? GameType { get; set; }
    [HtmlAttributeName("map")] public string? Map { get; set; }
    [HtmlAttributeName("class")] public string? CssClass { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "img";
        var src = !string.IsNullOrEmpty(Uri)
            ? Uri
            : $"/Maps/MapImage?gameType={GameType}&mapName={Map}";
        if (string.IsNullOrEmpty(src))
        {
            src = "/images/noimage.jpg";
        }
        output.Attributes.SetAttribute("src", src);
        output.Attributes.SetAttribute("alt", Map ?? "map");
        var style = "border: 5px solid #021a40; display: block; margin: auto;";
        output.Attributes.SetAttribute("style", style);
        if (!string.IsNullOrWhiteSpace(CssClass))
        {
            output.Attributes.SetAttribute("class", CssClass);
        }
        output.TagMode = TagMode.SelfClosing;
    }
}

[HtmlTargetElement("map-popularity")]
public class MapPopularityTagHelper : TagHelper
{
    [HtmlAttributeName("name")] public string Name { get; set; } = string.Empty;
    [HtmlAttributeName("like-percentage")] public double LikePercentage { get; set; }
    [HtmlAttributeName("dislike-percentage")] public double DislikePercentage { get; set; }
    [HtmlAttributeName("total-likes")] public double TotalLikes { get; set; }
    [HtmlAttributeName("total-dislikes")] public double TotalDislikes { get; set; }
    [HtmlAttributeName("votes")] public int TotalVotes { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        var html =
            $"<div class=\"progress\" id=\"progress-{Name}\">" +
            $"<div class=\"progress-bar bg-info\" role=\"progressbar\" style=\"width: {LikePercentage}%\" aria-valuenow=\"{TotalLikes}\" aria-valuemin=\"0\" aria-valuemax=\"{TotalVotes}\"></div>" +
            $"<div class=\"progress-bar bg-danger\" role=\"progressbar\" style=\"width: {DislikePercentage}%\" aria-valuenow=\"{TotalDislikes}\" aria-valuemin=\"0\" aria-valuemax=\"{TotalVotes}\"></div>" +
            "</div>" +
            $"<div class=\"m-t-sm\">{TotalLikes} likes and {TotalDislikes} dislikes out of {TotalVotes}</div>";
        output.Content.SetHtmlContent(html);
    }
}
