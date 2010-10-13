using System.Web.Mvc;
using System.Web.Security;

namespace MyMvcApplication.Controllers
{
    public class AccountController : Controller
    {
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult LogOn()
        {
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult LogOn(string username, string password)
        {
            if((username == "steve") && (password == "secret")) {
                FormsAuthentication.RedirectFromLoginPage(username, false);
            }

            ModelState.AddModelError("username", "Either the username or password is incorrect.");
            return View();
        }
    }
}