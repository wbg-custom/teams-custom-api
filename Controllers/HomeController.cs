using Microsoft.AspNetCore.Mvc;

namespace TeamsMessagingExtensionsSearchAuthConfig.Controllers
{
    public class HomeController : Controller
    {
        [Route("/Home/RazorView")]
        public ActionResult RazorView()
        {
            return View("RazorView");
        }

        [Route("/Home/CustomForm")]
        public ActionResult CustomForm()
        {
            return View("CustomForm");
        }

        [Route("/Home/HtmlPage")]
        public IActionResult HtmlPage()
        {
            return View("HtmlPage");
        }

        [Route("/Home/UploadPhoto")]
        public IActionResult UploadPhoto()
        {
            return View("UploadPhoto");
        }
    }
}
