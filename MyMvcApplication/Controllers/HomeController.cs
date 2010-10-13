using System.IO;
using System.Web;
using System.Web.Mvc;

namespace MyMvcApplication.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewData["Message"] = "Welcome to ASP.NET MVC!";
            return View();
        }

        public ActionResult DoStuffWithSessionAndCookies()
        {
            Session["myIncrementingSessionItem"] = ((int?)(Session["myIncrementingSessionItem"] ?? 0)) + 1;
            Response.Cookies.Add(new HttpCookie("mycookie", "myval"));
            return Content("OK");
        }

        [Authorize]        
        public ActionResult SecretAction()
        {
            return Content("Hello, you're logged in as " + User.Identity.Name);
        }
    }
}
