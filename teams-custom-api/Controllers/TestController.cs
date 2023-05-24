using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Net;
using System.Text.Json;
using teams_custom_api.Helpers;
using teams_custom_api.Models;

namespace teams_custom_api.Controllers
{
    [ApiController]
    public class TestController : Controller
    {
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
            if(!Request.Headers.TryGetValue("Authorization", out stringValue))
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
                    return Json($"Success: PostData: {JsonSerializer.Serialize(testPostModel)} {System.Environment.NewLine}Token:{tokenObj.Item2}");
                }
            }
        }
    }
}
