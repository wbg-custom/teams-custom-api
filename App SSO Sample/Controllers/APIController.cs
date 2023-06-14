using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using TeamsAuthSSO.Models;
using TeamsTabSSO.Constants;
using TeamsTabSSO.Helpers;

namespace TeamsAuthSSO.Controllers
{
    public class APIController : Controller
    {
        private IWebHostEnvironment _webHostEnvironment;
        public APIController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpPost]
        public async Task<JsonResult> SharePointUpload([FromForm] SharePointUploadInputObj inputObj)
        {
            string userAccessToken = UtilityHelper.GetTokenFromHeaders(Request);
            if (string.IsNullOrWhiteSpace(userAccessToken))
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return Json("Failed! Autherization header is missing.");
            }
            string spResourceUri = SharePointConstants.SharePointsiteUrl;
            Tuple<bool, string> tokenObj = await TokenHelper.GetAccessToken_FromSSO(userAccessToken, spResourceUri);

            if (inputObj.file != null && inputObj.file.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    inputObj.file.CopyTo(ms);
                    byte[] fileBytes = ms.ToArray();
                    //string s = Convert.ToBase64String(fileBytes);
                    //string fileUploadedPaths;
                    //string extension = Path.GetExtension(inputObj.file.FileName);
                    //string fileName = inputObj.file.FileName.Replace(extension, "");
                    ////string filePath = string.Format("{0}\\Files\\WBGMPMobileUpload\\{1}-{2:MMM-dd-yyyy-HH-mm-ss}{3}", this.LocalEnvironment.ContentRootPath, fileName, DateTime.Now, extension);
                    //string filePath = Path.Combine(_webHostEnvironment.WebRootPath, Path.Combine("Files", "WBGMPMobileUpload", string.Format("{0}-{1:MMM-dd-yyyy-HH-mm-ss}{2}", fileName, DateTime.Now, extension)));
                    //using (var fileStream = new FileStream(filePath, FileMode.Create))
                    //{
                    //    inputObj.file.CopyTo(fileStream);
                    //    fileStream.Flush();
                    //    fileStream.Close();
                    //}
                    //FileInfo fileInfo = new FileInfo(filePath);
                    //if (fileInfo.Length > 5000000)
                    //{
                    //    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    //    return Json("Failed! Image size can not be greater than 5 MB.");
                    //}
                    //fileUploadedPaths = filePath;

                    HttpResponseObj result = SharePointHelper.UploadFileSharePoint(tokenObj.Item2, inputObj.file.FileName, fileBytes);
                    if (result.isSuccess) Response.StatusCode = (int)HttpStatusCode.OK;
                    else Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return Json(result);
                }
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return (Json("Failed! Upload file."));
            }
        }
    }
}
