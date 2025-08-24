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
        // Prefer an explicit URI if supplied, otherwise construct the map image endpoint when we have enough context.
        var src = !string.IsNullOrWhiteSpace(Uri)
            ? Uri
            : (!string.IsNullOrWhiteSpace(GameType) && !string.IsNullOrWhiteSpace(Map)
                ? $"/Maps/MapImage?gameType={GameType}&mapName={Map}"
                : "/images/noimage.jpg");

        // Guard against accidental empty query parameters which would yield a 404 and broken image icon.
        if (src.Contains("/Maps/MapImage") && (string.IsNullOrWhiteSpace(GameType) || string.IsNullOrWhiteSpace(Map)))
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
        // Add client-side fallback in case the resolved image 404s at runtime.
        output.Attributes.SetAttribute("onerror", "this.onerror=null;this.src='/images/noimage.jpg';");
        output.TagMode = TagMode.SelfClosing;
    }
}
