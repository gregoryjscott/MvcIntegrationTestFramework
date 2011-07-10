// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NameValueCollectionConversions.cs" company="Public">
//   Free
// </copyright>
// <summary>
//   The name value collection conversions.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MvcIntegrationTestFramework
{
    using System;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Web.Routing;

    /// <summary>
    /// The name value collection conversions.
    /// </summary>
    public static class NameValueCollectionConversions
    {
        /// <summary>
        /// The convert from object.
        /// </summary>
        /// <param name="valueSource">The object from which to extract values.</param>
        /// <returns>
        /// A name vlaue collection containing the values from the specified object.
        /// </returns>
        public static NameValueCollection ConvertFromObject(object valueSource)
        {
            if (valueSource == null)
            {
                throw new ArgumentNullException("valueSource");
            }

            var nvc = new NameValueCollection();
            var dict = new RouteValueDictionary(valueSource);

            foreach (var kvp in dict.Where(kvp => kvp.Value != null))
            {
                if (kvp.Value.GetType().Name.Contains("Anonymous"))
                {
                    var prefix = kvp.Key + ".";
                    foreach (var innerkvp in new RouteValueDictionary(kvp.Value))
                    {
                        nvc.Add(prefix + innerkvp.Key, innerkvp.Value.ToString());
                    }
                }
                else
                {
                    nvc.Add(kvp.Key, kvp.Value.ToString());
                }
            }

            return nvc;
        }
    }
}