using Microsoft.AspNetCore.Razor.TagHelpers;

namespace XtremeIdiots.Portal.Web.Helpers;

[HtmlTargetElement("confidence-label")]
public class ConfidenceLabelTagHelper : TagHelper
{
    [HtmlAttributeName("score")] public int Score { get; set; }
    [HtmlAttributeName("last-used")] public DateTime LastUsed { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        if (Score == 0)
        {
            output.Attributes.SetAttribute("class", "label label-default");
            output.Attributes.SetAttribute("data-toggle", "tooltip");
            output.Attributes.SetAttribute("data-placement", "bottom");
            output.Attributes.SetAttribute("title", "There is not yet have enough data to score this");
            output.Content.SetHtmlContent("N/A - Unknown Confidence");
            return;
        }

        string html;
        if (Score is > 0 and < 2)
        {
            html = "<span class=\"label label-danger\" data-toggle=\"tooltip\" data-placement=\"bottom\" title=\"This data record has only been linked >0 and <2 times\">Very Low Confidence</span>";
        }
        else if (Score is > 2 and < 5)
        {
            html = "<span class=\"label label-warning\" data-toggle=\"tooltip\" data-placement=\"bottom\" title=\"This data record has been linked >2 and <5 times\">Average Confidence</span>";
        }
        else if (LastUsed < DateTime.UtcNow.AddMonths(-6))
        {
            html = "<span class=\"label label-warning\" data-toggle=\"tooltip\" data-placement=\"bottom\" title=\"This data record has not been used in over 6 months\">Average Confidence</span>";
        }
        else
        {
            html = $"<span class=\"label label-success\" data-toggle=\"tooltip\" data-placement=\"bottom\" title=\"This data record has been linked {Score} times\">High Confidence</span>";
        }
        output.Content.SetHtmlContent(html);
    }
}
