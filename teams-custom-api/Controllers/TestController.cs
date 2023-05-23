﻿using Microsoft.AspNetCore.Mvc;
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
        public JsonResult Index(TestPostModel testPostModel)
        {
            StringValues stringValue;
            if(!Request.Headers.TryGetValue("Authorization", out stringValue))
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return Json("Failed! Authorization header is missing.");
            }
            else
            {
                Tuple<bool, string> tokenObj = TokenHelper.GetAccessToken_MSAL(stringValue, "api://teamscustomapp.azurewebsites.net/e4f30e80-248c-4421-9ff8-ec1050d877b0");
                if (!tokenObj.Item1)
                {
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return Json(tokenObj.Item2);
                }
                else
                {
                    Response.StatusCode = (int)HttpStatusCode.OK;
                    return Json($"Success: {JsonSerializer.Serialize(testPostModel)}");
                }
            }
        }
    }
}
