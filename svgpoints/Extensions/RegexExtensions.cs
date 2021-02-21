using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace svgpoints.Extensions
{
    public static class RegexExtensions
    {
        /// <summary>
        /// Gets list of matches from Match so you can use a foreach statement
        /// on it.
        /// </summary>
        /// <param name="match">Starting match to get all matches for</param>
        /// <returns>Enumerable list of all matches</returns>
        public static IEnumerable<Match> GetMatches(this Match match)
        {
            var matchHold = match;

            while (matchHold.Success)
            {
                yield return matchHold;
                matchHold = matchHold.NextMatch();
            }
        }
    }
}
