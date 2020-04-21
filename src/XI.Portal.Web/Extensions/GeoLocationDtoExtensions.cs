using FM.GeoLocation.Contract.Models;
using Microsoft.AspNetCore.Html;

namespace XI.Portal.Web.Extensions
{
    public static class GeoLocationDtoExtensions
    {
        public static HtmlString FlagImage(this GeoLocationDto geoLocationDto)
        {
            return !string.IsNullOrWhiteSpace(geoLocationDto.CountryCode)
                ? new HtmlString($"<img src=\"/images/flags/{geoLocationDto.CountryCode.ToLower()}.png\" />")
                : new HtmlString("<img src=\"/images/flags/unknown.png\" />");
        }
    }
}