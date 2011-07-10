// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LastRequestData.cs" company="Public">
//   Free
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace MvcIntegrationTestFramework.Interception
{
    using System.Web;
    using System.Web.Mvc;
    using System.Web.SessionState;

    /// <summary>
    /// A place to store information about each request as it is captured.
    ///   Static because HttpRuntime.ProcessRequest() exposes no convenient hooks for intercepting the
    ///   request processing pipeline, so we're statically attaching an interceptor to all loaded controllers
    /// </summary>
    internal static class LastRequestData
    {
        /// <summary>
        /// Gets or sets ActionExecutedContext.
        /// </summary>
        public static ActionExecutedContext ActionExecutedContext { get; set; }

        /// <summary>
        /// Gets or sets HttpSessionState.
        /// </summary>
        public static HttpSessionState HttpSessionState { get; set; }

        /// <summary>
        /// Gets or sets Response.
        /// </summary>
        public static HttpResponse Response { get; set; }

        /// <summary>
        /// Gets or sets ResultExecutedContext.
        /// </summary>
        public static ResultExecutedContext ResultExecutedContext { get; set; }

        /// <summary>
        /// The reset.
        /// </summary>
        public static void Reset()
        {
            ActionExecutedContext = null;
            ResultExecutedContext = null;
            HttpSessionState = null;
            Response = null;
        }
    }
}