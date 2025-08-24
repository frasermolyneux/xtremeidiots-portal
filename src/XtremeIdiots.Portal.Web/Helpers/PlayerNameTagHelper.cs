using Microsoft.AspNetCore.Razor.TagHelpers;

namespace XtremeIdiots.Portal.Web.Helpers;

[HtmlTargetElement("player-name")]
public class PlayerNameTagHelper : TagHelper
{
    [HtmlAttributeName("value")] public string? Value { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        if (string.IsNullOrWhiteSpace(Value))
        {
            output.Content.SetContent(string.Empty);
            return;
        }

        var cleaned = Value;
        var toRemove = new[] { "^1", "^2", "^3", "^4", "^5", "^6", "^7", "^8", "^9" };
        foreach (var code in toRemove)
        {
            cleaned = cleaned.Replace(code, string.Empty);
        }

        output.Content.SetContent(cleaned);
    }
}
