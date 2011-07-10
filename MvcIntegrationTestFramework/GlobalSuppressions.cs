// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlobalSuppressions.cs" company="Public">
//   Free
// </copyright>
// <summary>
//   GlobalSuppressions.cs
// </summary>
// --------------------------------------------------------------------------------------------------------------------

[assembly:
    System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", 
        "CA2000:Dispose objects before losing scope", Scope = "member", 
        Target = "MvcIntegrationTestFramework.Hosting.AppHost.#GetApplicationInstance()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Scope = "member", Target = "MvcIntegrationTestFramework.Browsing.BrowsingSession.#ProcessRequest(System.String,System.Web.Mvc.HttpVerbs,System.Collections.Specialized.NameValueCollection,System.Collections.Specialized.NameValueCollection)")]
