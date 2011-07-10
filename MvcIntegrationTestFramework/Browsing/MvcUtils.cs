// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MvcUtils.cs" company="Public">
//   Free
// </copyright>
// <summary>
//   The mvc utils.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MvcIntegrationTestFramework.Browsing
{
    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The mvc utils.
    /// </summary>
    public static class MvcUtils
    {
        /// <summary>
        /// The extract anti forgery token.
        /// </summary>
        /// <param name="htmlResponseText">The html response text from which to extract the token.</param>
        /// <returns>
        /// The extracted anti forgery token when it is found else null.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="htmlResponseText"/> is null.</exception>
        public static string ExtractAntiForgeryToken(string htmlResponseText)
        {
            if (htmlResponseText == null)
            {
                throw new ArgumentNullException("htmlResponseText");
            }

            const string Pattern =
                @"\<input name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]+)"" \/\>";

            var match = Regex.Match(Pattern, htmlResponseText);
            return match.Success ? match.Groups[1].Captures[0].Value : null;
        }
    }
}