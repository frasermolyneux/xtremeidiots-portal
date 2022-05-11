using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace XI.Portal.Web.Extensions
{
    public static class MapImageHtmlExtensions
    {
        public static HtmlString MapImage(this IHtmlHelper html, string mapImageUri)
        {
            if (string.IsNullOrEmpty(mapImageUri))
            {
                return new HtmlString(
                $"<img style=\"border: 5px solid #021a40; display: block; margin: auto;\" src=\"/images/noimage.jpg\" alt=\"noimage\" />");
            }
            else
            {
                return new HtmlString(
                $"<img style=\"border: 5px solid #021a40; display: block; margin: auto;\" src=\"{mapImageUri}\" />");
            }
        }

        public static HtmlString MapImage(this IHtmlHelper html, string gameType, string mapName)
        {
            return new HtmlString(
                $"<img style=\"border: 5px solid #021a40; display: block; margin: auto;\" src=\"/Maps/MapImage?gameType={gameType}&mapName={mapName}\" alt=\"{mapName}\" />");
        }

        public static HtmlString MapPopularity(this IHtmlHelper html, string mapName, double likePercentage, double dislikePercentage, double totalLikes, double totalDislikes, int totalVotes)
        {
            return new HtmlString(
                $"<div class=\"progress\" id=\"progress-{mapName}\">" +
                $"<div class=\"progress-bar bg-info\" role=\"progressbar\" style=\"width: {likePercentage}%\" aria-valuenow=\"{totalLikes}\" aria-valuemin=\"0\" aria-valuemax=\"{totalVotes}\"></div>" +
                $"<div class=\"progress-bar bg-danger\" role=\"progressbar\" style=\"width: {dislikePercentage}%\" aria-valuenow=\"{totalDislikes}\" aria-valuemin=\"0\" aria-valuemax=\"{totalVotes}\"></div>" +
                "</div>" +
                $"<div class=\"m-t-sm\">{totalLikes} likes and {totalDislikes} dislikes out of {totalVotes}</div>");
        }
    }
}