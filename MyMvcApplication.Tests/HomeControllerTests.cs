using System.Web.Mvc;
using MvcIntegrationTestFramework.Browsing;
using MvcIntegrationTestFramework.Hosting;
using NUnit.Framework;

namespace MyMvcApplication.Tests
{
    [TestFixture]
    public class HomeControllerTests
    {
        [Test]
        public void Root_Url_Renders_Index_View()
        {
            AppHost.Simulate().BrowsingSession(browsingSession => {
                // Request the root URL
                RequestResult result = browsingSession.Get("");

                // Can make assertions about the ActionResult...
                var viewResult = (ViewResult) result.ActionExecutedContext.Result;
                Assert.AreEqual("Index", viewResult.ViewName);
                Assert.AreEqual("Welcome to ASP.NET MVC!", viewResult.ViewData["Message"]);

                // ... or can make assertions about the rendered HTML
                Assert.IsTrue(result.ResponseText.Contains("<!DOCTYPE html"));
            });
        }

        [Test]
        public void WorkWithCookiesAndSession()
        {
            AppHost.Simulate().BrowsingSession(browsingSession =>
            {
                string url = "home/DoStuffWithSessionAndCookies";
                browsingSession.Get(url);

                // Can make assertions about cookies
                Assert.AreEqual("myval", browsingSession.Cookies["mycookie"].Value);
                
                // Can read Session as long as you've already made at least one request
                // (you can also write to Session from your test if you want)
                Assert.AreEqual(1, browsingSession.Session["myIncrementingSessionItem"]);

                // Session values persist within a browsingSession
                browsingSession.Get(url);
                Assert.AreEqual(2, browsingSession.Session["myIncrementingSessionItem"]);
                browsingSession.Get(url);
                Assert.AreEqual(3, browsingSession.Session["myIncrementingSessionItem"]);
            });
        }

        [Test]
        public void LogInProcess()
        {
            string securedActionUrl = "/home/SecretAction";

            AppHost.Simulate().BrowsingSession(browsingSession =>
            {
                // First try to request a secured page without being logged in                
                RequestResult initialRequestResult = browsingSession.Get(securedActionUrl);
                string loginRedirectUrl = initialRequestResult.Response.RedirectLocation;
                Assert.IsTrue(loginRedirectUrl.StartsWith("/Account/LogOn"), "Didn't redirect to logon page");

                // Now follow redirection to logon page
                string loginFormResponseText = browsingSession.Get(loginRedirectUrl).ResponseText;
                string suppliedAntiForgeryToken = MvcUtils.ExtractAntiForgeryToken(loginFormResponseText);

                // Now post the login form, including the verification token
                RequestResult loginResult = browsingSession.Post(loginRedirectUrl, new
                                                                                       {
                                                                                           UserName = "steve",
                                                                                           Password = "secret",
                                                                                           __RequestVerificationToken = suppliedAntiForgeryToken
                                                                                       });
                string afterLoginRedirectUrl = loginResult.Response.RedirectLocation;
                Assert.AreEqual(securedActionUrl, afterLoginRedirectUrl, "Didn't redirect back to SecretAction");

                // Check that we can now follow the redirection back to the protected action, and are let in
                RequestResult afterLoginResult = browsingSession.Get(securedActionUrl);
                Assert.AreEqual("Hello, you're logged in as steve", afterLoginResult.ResponseText);
            });
        }
    }
}
