using Microsoft.AspNetCore.Razor.TagHelpers;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;

namespace XtremeIdiots.Portal.Web.Helpers;

[HtmlTargetElement("game-type-icon")]
public class GameTypeIconTagHelper : TagHelper
{
    [HtmlAttributeName("game")] public GameType Game { get; set; }
    [HtmlAttributeName("external")] public bool External { get; set; }
    [HtmlAttributeName("size")] public int Size { get; set; } = 16;
    [HtmlAttributeName("class")] public string? CssClass { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "img";
        var baseUrl = External ? "https://portal.xtremeidiots.com" : string.Empty;
        output.Attributes.SetAttribute("src", $"{baseUrl}/images/game-icons/{Game}.png");
        output.Attributes.SetAttribute("alt", Game.ToString());
        output.Attributes.SetAttribute("width", Size.ToString());
        output.Attributes.SetAttribute("height", Size.ToString());
        if (!string.IsNullOrWhiteSpace(CssClass)) output.Attributes.SetAttribute("class", CssClass);
        output.TagMode = TagMode.SelfClosing;
    }
}
