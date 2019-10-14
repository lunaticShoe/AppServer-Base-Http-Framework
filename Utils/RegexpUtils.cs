using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SNMPAgent.Utils
{
    public static class RegexpUtils
    {
        public static List<string> ListMatches(string expr, string text)
        {
            var matches = Regex.Matches(
                    text,
                    expr,
                    RegexOptions.Multiline);

            var res = new List<string>();

            foreach (Match match in matches)
                res.Add(match.Value);

            return res;
        }
    }
}
