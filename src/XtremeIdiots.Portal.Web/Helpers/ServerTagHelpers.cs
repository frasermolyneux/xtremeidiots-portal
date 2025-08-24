using Microsoft.AspNetCore.Razor.TagHelpers;

namespace XtremeIdiots.Portal.Web.Helpers;

[HtmlTargetElement("server-host")]
public class ServerHostTagHelper : TagHelper
{
    [HtmlAttributeName("host")] public string? Host { get; set; }
    [HtmlAttributeName("port")] public int Port { get; set; }
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        if (string.IsNullOrWhiteSpace(Host) && Port == 0)
        {
            output.SuppressOutput();
            return;
        }

        if (!string.IsNullOrWhiteSpace(Host) && Port == 0)
        {
            output.Content.SetContent(Host);
        }
        else
        {
            output.Content.SetContent($"{Host}:{Port}");
        }
    }
}

[HtmlTargetElement("server-name")]
public class ServerNameTagHelper : TagHelper
{
    [HtmlAttributeName("title")] public string? Title { get; set; }
    [HtmlAttributeName("live-title")] public string? LiveTitle { get; set; }
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        var value = string.IsNullOrWhiteSpace(LiveTitle) ? Title ?? string.Empty : LiveTitle;
        var toRemove = new[] { "^1", "^2", "^3", "^4", "^5", "^6", "^7", "^8", "^9" };
        foreach (var code in toRemove)
        {
            value = value?.Replace(code, string.Empty);
        }

        output.Content.SetContent(value ?? string.Empty);
    }
}

[HtmlTargetElement("server-link")]
public class ServerLinkTagHelper : TagHelper
{
    [HtmlAttributeName("type")] public string Type { get; set; } = string.Empty; // gametracker|hlsw|steam
    [HtmlAttributeName("game")] public string? Game { get; set; }
    [HtmlAttributeName("host")] public string Host { get; set; } = string.Empty;
    [HtmlAttributeName("port")] public int Port { get; set; }
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "a";
        output.Attributes.SetAttribute("style", "margin:5px");
        switch (Type.ToLowerInvariant())
        {
            case "gametracker":
                output.Attributes.SetAttribute("href", $"https://www.gametracker.com/server_info/{Host}:{Port}");
                output.Attributes.SetAttribute("target", "_blank");
                output.Content.SetHtmlContent("<img src=\"/images/service-icons/gametracker.png\" alt=\"gametracker\"/>");
                break;
            case "hlsw":
                var hlswGame = Game switch
                {
                    "CallOfDuty2" => "CoD2",
                    "CallOfDuty4" => "CoD4",
                    "CallOfDuty5" => "CoDWW",
                    _ => null
                };
                if (hlswGame == null)
                {
                    output.SuppressOutput();
                    return;
                }
                output.Attributes.SetAttribute("href", $"hlsw://{Host}:{Port}?Game={hlswGame}");
                output.Content.SetHtmlContent("<img src=\"/images/service-icons/hlsw.png\" alt=\"hlsw\"/>");
                break;
            case "steam":
                output.Attributes.SetAttribute("href", $"steam://connect/{Host}:{Port}");
                output.Content.SetHtmlContent("<img src=\"/images/service-icons/steam.png\" alt=\"steam\"/>");
                break;
            default:
                output.SuppressOutput();
                return;
        }
    }
}
