// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BrowsingSession.cs" company="Public">
//   Free
// </copyright>
// <summary>
//   The browsing session.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MvcIntegrationTestFramework.Browsing
{
    using System;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.SessionState;

    using MvcIntegrationTestFramework.Interception;

    /// <summary>
    /// The browsing session.
    /// </summary>
    public class BrowsingSession
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrowsingSession"/> class.
        /// </summary>
        public BrowsingSession()
        {
            this.Cookies = new HttpCookieCollection();
        }

        /// <summary>
        /// Gets Cookies.
        /// </summary>
        public HttpCookieCollection Cookies { get; private set; }

        /// <summary>
        /// Gets Session.
        /// </summary>
        public HttpSessionState Session { get; private set; }

        /// <summary>
        /// Sends a GET request to the specified URL.
        /// </summary>
        /// <param name="url">The URL of the GET request.</param>
        /// <returns>A RequestResult object containing the result of the request execution.</returns>
        public RequestResult Get(string url)
        {
            return this.ProcessRequest(url, HttpVerbs.Get, new NameValueCollection());
        }

        /// <summary>
        /// Sends a POST request to the specified URL.
        /// </summary>
        /// <param name="url">The URL of the POST request.</param>
        /// <param name="formData">The form data, which can be an anonymous type of the form new { Key = "Value" }</param>
        /// <returns>A RequestResult object containing the result of the request execution.</returns>
        /// <example>
        ///   <code>
        /// var result = Post("registration/create", new
        /// {
        ///   Form = new
        ///   {
        ///     InvoiceNumber = "10000",
        ///     AmountDue = "10.00",
        ///     Email = "chriso@innovsys.com",
        ///     Password = "welcome",
        ///     ConfirmPassword = "welcome"
        ///   }
        /// });
        ///   </code>
        ///   </example>
        public RequestResult Post(string url, object formData)
        {
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }

            var formNameValueCollection = NameValueCollectionConversions.ConvertFromObject(formData);
            return this.ProcessRequest(url, HttpVerbs.Post, formNameValueCollection);
        }

        /// <summary>
        /// Adds any new cookies to the cookie collection.
        /// </summary>
        private void AddAnyNewCookiesToCookieCollection()
        {
            if (LastRequestData.Response == null)
            {
                return;
            }

            var lastResponseCookies = LastRequestData.Response.Cookies;

            foreach (string cookieName in lastResponseCookies)
            {
                var cookie = lastResponseCookies[cookieName];

                if (this.Cookies[cookieName] != null)
                {
                    this.Cookies.Remove(cookieName);
                }

                if ((cookie.Expires == default(DateTime)) || (cookie.Expires > DateTime.Now))
                {
                    this.Cookies.Add(cookie);
                }
            }
        }

        /// <summary>
        /// Processes the specified request.
        /// </summary>
        /// <param name="url">The request URL.</param>
        /// <param name="httpVerb">The HTTP verb of the request.</param>
        /// <param name="formValues">The form values.</param>
        /// <returns>A RequestResult object containing the result of the request execution.</returns>
        private RequestResult ProcessRequest(
            string url, HttpVerbs httpVerb = HttpVerbs.Get, NameValueCollection formValues = null)
        {
            return this.ProcessRequest(url, httpVerb, formValues, null);
        }

        /// <summary>
        /// Processes the specified request.
        /// </summary>
        /// <param name="url">The URL of the request.</param>
        /// <param name="httpVerb">The HTTP verb of the request.</param>
        /// <param name="formValues">The form values.</param>
        /// <param name="headers">The request headers.</param>
        /// <returns>A RequestResult object containing the result of the request execution.</returns>
        private RequestResult ProcessRequest(
            string url, HttpVerbs httpVerb, NameValueCollection formValues, NameValueCollection headers)
        {
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }

            // Fix up URLs that incorrectly start with / or ~/
            if (url.StartsWith("~/"))
            {
                url = url.Substring(2);
            }
            else if (url.StartsWith("/"))
            {
                url = url.Substring(1);
            }

            // Parse out the querystring if provided
            var query = string.Empty;
            var querySeparatorIndex = url.IndexOf("?");
            
            if (querySeparatorIndex >= 0)
            {
                query = url.Substring(querySeparatorIndex + 1);
                url = url.Substring(0, querySeparatorIndex);
            }

            // Perform the request
            LastRequestData.Reset();
            var output = new StringWriter();
            var httpVerbName = httpVerb.ToString().ToLower(CultureInfo.InvariantCulture);
            var workerRequest = new SimulatedWorkerRequest(url, query, output, this.Cookies, httpVerbName, formValues, headers);
            HttpRuntime.ProcessRequest(workerRequest);

            // Capture the output
            this.AddAnyNewCookiesToCookieCollection();
            this.Session = LastRequestData.HttpSessionState;
            return new RequestResult
                {
                    ResponseText = output.ToString(), 
                    ActionExecutedContext = LastRequestData.ActionExecutedContext, 
                    ResultExecutedContext = LastRequestData.ResultExecutedContext, 
                    Response = LastRequestData.Response, 
                };
        }
    }
}