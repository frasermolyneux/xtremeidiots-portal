using Microsoft.AspNetCore.Razor.TagHelpers;

namespace XtremeIdiots.Portal.Web.Helpers;

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
        // Some APIs may return proportions (0-1). Detect and scale for display.
        var likePct = LikePercentage;
        var dislikePct = DislikePercentage;
        if (likePct <= 1 && dislikePct <= 1 && (likePct + dislikePct) <= 2)
        {
            likePct *= 100;
            dislikePct *= 100;
        }

        // If API provided 0 percentages but we have counts, derive them.
        if (TotalVotes > 0 && likePct == 0 && dislikePct == 0 && (TotalLikes > 0 || TotalDislikes > 0))
        {
            likePct = TotalLikes / TotalVotes * 100d;
            dislikePct = TotalDislikes / TotalVotes * 100d;
        }

        // Ensure tiny non-zero values are visible.
        if (likePct is > 0 and < 1)
        {
            likePct = 1;
        }

        if (dislikePct is > 0 and < 1)
        {
            dislikePct = 1;
        }

        var html =
            $"<div class=\"progress\" id=\"progress-{Name}\" style=\"width:100%;min-width:140px;max-width:260px;\">" +
            $"<div class=\"progress-bar bg-info\" role=\"progressbar\" style=\"width: {likePct:F2}%\" aria-valuenow=\"{TotalLikes}\" aria-valuemin=\"0\" aria-valuemax=\"{TotalVotes}\"></div>" +
            $"<div class=\"progress-bar bg-danger\" role=\"progressbar\" style=\"width: {dislikePct:F2}%\" aria-valuenow=\"{TotalDislikes}\" aria-valuemin=\"0\" aria-valuemax=\"{TotalVotes}\"></div>" +
            "</div>" +
            $"<div class=\"m-t-sm\">{TotalLikes} likes and {TotalDislikes} dislikes out of {TotalVotes}</div>";
        output.Content.SetHtmlContent(html);
    }
}
