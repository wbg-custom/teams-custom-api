using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Net;
using System.Text.Json;
using TeamsAuthSSO.Models;
using TeamsTabSSO.Helpers;

namespace TeamsAuthSSO.Controllers
{
    public class TestController : Controller
    {
        private readonly IConfiguration _config;
        public TestController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        [Route("/test/get")]
        public JsonResult TestGet()
        {
            return Json("Success!");
        }

        [HttpPost]
        [Route("/test/post")]
        public async Task<JsonResult> IndexAsync(TestPostModel testPostModel)
        {
            StringValues stringValue;
            if (!Request.Headers.TryGetValue("Authorization", out stringValue))
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return Json("Failed! Authorization header is missing.");
            }
            else
            {
                Tuple<bool, string> tokenObj = await TokenHelper.GetAccessToken_FromSSO(stringValue.ToString(), "https://wbgcustomoutlook.onmicrosoft.com");
                if (!tokenObj.Item1)
                {
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return Json(tokenObj.Item2);
                }
                else
                {
                    Response.StatusCode = (int)HttpStatusCode.OK;
                    return Json($"Success: PostData: {JsonSerializer.Serialize(testPostModel)} Token:{tokenObj.Item2}");
                }
            }
        }

        [Route("/test/UploadPhoto")]
        public IActionResult UploadPhoto()
        {
            ViewBag.SiteUrl = _config.GetValue<string>("SiteUrl");
            return View();//"UploadPhoto"
        }

        [Route("/test/msGraphToken")]
        public IActionResult msGraphToken()
        {
            ViewBag.SiteUrl = _config.GetValue<string>("SiteUrl");
            return View();//"UploadPhoto"
        }
    }
}
