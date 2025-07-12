using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;

namespace XtremeIdiots.Portal.Web.Extensions
{
    public static class DateTimeHtmlExtensions
    {
        public static HtmlString ToUserTime(this IHtmlHelper html, ClaimsPrincipal user, DateTime dateTime)
        {
            if (user == null)
                return new HtmlString(dateTime.ToString());

            var timezoneClaim = user.Claims.SingleOrDefault(c => c.Type == UserProfileClaimType.TimeZone);

            if (timezoneClaim == null)
                return new HtmlString(dateTime.ToString());

            try
            {
                var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezoneClaim.Value);
                var userDateTime = TimeZoneInfo.ConvertTime(dateTime, timeZoneInfo);
                return new HtmlString(userDateTime.ToString());
            }
            catch (Exception)
            {
                return new HtmlString(dateTime.ToString());
            }
        }
    }
}
