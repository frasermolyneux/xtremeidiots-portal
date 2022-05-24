using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace XtremeIdiots.Portal.RepositoryFunc.Extensions
{
    public static class StringExtensions
    {
        public static string NormalizeName(this string playerName)
        {
            var toRemove = new List<string> { "^0", "^1", "^2", "^3", "^4", "^5", "^6", "^7", "^8", "^9" };

            var toReturn = playerName.ToUpper();
            toReturn = toRemove.Aggregate(toReturn, (current, val) => current.Replace(val, ""));

            if (toReturn.StartsWith("["))
            {
                var regex = new Regex("^(\\[.*\\])");
                var match = regex.Match(toReturn);

                if (match.Success)
                {
                    var matchedTag = match.Groups[1];
                    toReturn = toReturn.Replace(matchedTag.ToString(), "");
                }
            }

            toReturn = toReturn.Trim();
            return toReturn;
        }
    }
}