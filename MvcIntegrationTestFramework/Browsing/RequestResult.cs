// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequestResult.cs" company="Public">
//   Free
// </copyright>
// <summary>
//   Represents the result of a simulated request
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MvcIntegrationTestFramework.Browsing
{
    using System.Web;
    using System.Web.Mvc;

    /// <summary>
    /// Represents the result of a simulated request
    /// </summary>
    public class RequestResult
    {
        /// <summary>
        /// Gets or sets ActionExecutedContext.
        /// </summary>
        public ActionExecutedContext ActionExecutedContext { get; set; }

        /// <summary>
        /// Gets or sets Response.
        /// </summary>
        public HttpResponse Response { get; set; }

        /// <summary>
        /// Gets or sets ResponseText.
        /// </summary>
        public string ResponseText { get; set; }

        /// <summary>
        /// Gets or sets ResultExecutedContext.
        /// </summary>
        public ResultExecutedContext ResultExecutedContext { get; set; }
    }
}