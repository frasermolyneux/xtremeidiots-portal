using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MX.GeoLocation.Abstractions.Models.V1;

namespace XtremeIdiots.Portal.Web.Helpers;

[HtmlTargetElement("flag-image")]
public class FlagImageTagHelper : TagHelper
{
    [HtmlAttributeName("country-code")] public string? CountryCode { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "img";
        var code = string.IsNullOrWhiteSpace(CountryCode) ? "unknown" : CountryCode.ToLower();
        output.Attributes.SetAttribute("src", $"/images/flags/{code}.png");
        output.TagMode = TagMode.SelfClosing;
    }
}

[HtmlTargetElement("location-summary", Attributes = "geo-model")]
public class LocationSummaryTagHelper : TagHelper
{
    [HtmlAttributeName("geo-model")] public GeoLocationDto? Geo { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        if (Geo == null)
        {
            output.Content.SetContent("Unknown");
            return;
        }

        var text = !string.IsNullOrWhiteSpace(Geo.CityName) && !string.IsNullOrWhiteSpace(Geo.CountryName)
            ? $"{Geo.CityName}, {Geo.CountryName}"
            : !string.IsNullOrWhiteSpace(Geo.CountryCode)
                ? Geo.CountryCode!
                : !string.IsNullOrWhiteSpace(Geo.RegisteredCountry)
                    ? Geo.RegisteredCountry!
                    : "Unknown";
        output.Content.SetContent(text);
    }
}

[HtmlTargetElement("ip-address", Attributes = "ip")]
public class IpAddressTagHelper : TagHelper
{
    [HtmlAttributeName("ip")] public string? Ip { get; set; }
    [HtmlAttributeName("geo")] public GeoLocationDto? Geo { get; set; }
    [HtmlAttributeName("risk")] public int? Risk { get; set; }
    [HtmlAttributeName("is-proxy")] public bool? IsProxy { get; set; }
    [HtmlAttributeName("is-vpn")] public bool? IsVpn { get; set; }
    [HtmlAttributeName("proxy-type")] public string? ProxyType { get; set; }
    [HtmlAttributeName("link-to-details")] public bool LinkToDetails { get; set; } = true;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        if (string.IsNullOrEmpty(Ip))
        {
            output.SuppressOutput();
            return;
        }

        var flag = Geo?.CountryCode;
        var code = string.IsNullOrEmpty(flag) ? "unknown" : flag.ToLower();
        var parts = new List<string>
        {
            $"<img src=\"/images/flags/{code}.png\" />",
            LinkToDetails ? $"<a href=\"/IPAddresses/Details?ipAddress={Ip}\">{Ip}</a>" : Ip
        };

        if (Risk.HasValue)
        {
            var riskClass = Risk.Value switch
            {
                >= 80 => "text-bg-danger",
                >= 50 => "text-bg-warning",
                >= 25 => "text-bg-info",
                _ => "text-bg-success"
            };
            parts.Add($"<span class=\"badge rounded-pill {riskClass}\">Risk: {Risk}</span>");
        }

        if (!string.IsNullOrEmpty(ProxyType))
        {
            parts.Add($"<span class=\"badge rounded-pill text-bg-primary\">{ProxyType}</span>");
        }
        if (IsProxy == true)
        {
            parts.Add("<span class=\"badge rounded-pill text-bg-danger\">Proxy</span>");
        }

        if (IsVpn == true)
        {
            parts.Add("<span class=\"badge rounded-pill text-bg-warning\">VPN</span>");
        }


        output.Content.SetHtmlContent(string.Join(' ', parts));
    }
}