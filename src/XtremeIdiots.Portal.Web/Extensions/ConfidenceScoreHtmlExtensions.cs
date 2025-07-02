using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace XtremeIdiots.Portal.Web.Extensions
{
    public static class ConfidenceScoreHtmlExtensions
    {
        public static HtmlString ToConfidenceLabel(this IHtmlHelper html, int confidenceScore, DateTime lastUsed)
        {
            if (confidenceScore == 0)
            {
                return new HtmlString("<span class=\"label label-default\" data-toggle=\"tooltip\" data-placement=\"bottom\" title=\"There is not yet have enough data to score this\">N/A - Unknown Confidence</span>");
            }
            else if (confidenceScore > 0 && confidenceScore < 2)
            {
                return new HtmlString("<span class=\"label label-danger\" data-toggle=\"tooltip\" data-placement=\"bottom\" title=\"This data record has only been linked >0 and <2 times\">Very Low Confidence</span>");
            }
            else if (confidenceScore > 2 && confidenceScore < 5)
            {
                return new HtmlString("<span class=\"label label-warning\" data-toggle=\"tooltip\" data-placement=\"bottom\" title=\"This data record has been linked >2 and <5 times\">Average Confidence</span>");
            }
            else
            {
                if (lastUsed < DateTime.UtcNow.AddMonths(-6))
                {
                    return new HtmlString("<span class=\"label label-warning\" data-toggle=\"tooltip\" data-placement=\"bottom\" title=\"This data record has not been used in over 6 months\">Average Confidence</span>");
                }
                else
                {
                    return new HtmlString($"<span class=\"label label-success\" data-toggle=\"tooltip\" data-placement=\"bottom\" title=\"This data record has been linked {confidenceScore} times\">High Confidence</span>");
                }
            }
        }
    }
}
