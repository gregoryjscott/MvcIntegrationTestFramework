using System.Collections.Specialized;
using System.Web.Routing;

namespace MvcIntegrationTestFramework
{
    public static class NameValueCollectionConversions
    {
        public static NameValueCollection ConvertFromObject(object anonymous)
        {
            var nvc = new NameValueCollection();
            var dict = new RouteValueDictionary(anonymous);

            foreach (var kvp in dict)
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
