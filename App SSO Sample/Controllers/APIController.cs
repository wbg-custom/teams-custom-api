using App_SSO_Sample.Helpers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        [Route("/sharepoint/upload")]
        public async Task<JsonResult> SharePointUpload([FromForm] FileUploadInputObj inputObj)
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

        [HttpPost]
        [Route("/blobstorage/upload")]
        public async Task<JsonResult> AzureStorageUpload([FromForm] FileUploadInputObj inputObj)
        {
            //string userAccessToken = UtilityHelper.GetTokenFromHeaders(Request);
            //if (string.IsNullOrWhiteSpace(userAccessToken))
            //{
            //    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            //    return Json("Failed! Autherization header is missing.");
            //}
            //else 
            //if (string.IsNullOrWhiteSpace(inputObj.TeamId))
            //{
            //    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            //    return Json("Failed! TeamId is missing.");
            //}
            //else 
            if (string.IsNullOrWhiteSpace(inputObj.ChannelId))
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return Json("Failed! ChannelId is missing.");
            }
            else if (inputObj.file != null && inputObj.file.Length > 0)
            {
                //string spResourceUri = SharePointConstants.SharePointsiteUrl;
                //Tuple<bool, string> tokenObj = await TokenHelper.GetAccessToken_FromSSO(userAccessToken, spResourceUri);

                //if (tokenObj.Item1)
                //{
                    using (var ms = new MemoryStream())
                    {
                        inputObj.file.CopyTo(ms);
                        byte[] fileBytes = ms.ToArray();
                        string fileName = inputObj.file.FileName;
                        AzurestorageHelper objAzureStorage = new AzurestorageHelper();
                        string result = await objAzureStorage.UploadFromBinaryDataAsync($"{inputObj.TeamId}/{inputObj.ChannelId}/{fileName}", fileBytes);
                        if (!string.IsNullOrEmpty(result))
                        {
                            Response.StatusCode = (int)HttpStatusCode.OK;
                            inputObj.Name = fileName;
                            await AzureSearchHelper.AddAzureSearchIndex(inputObj, result);
                        }
                        else Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        return Json(result);
                    }
                //}
                //else
                //{
                //    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                //    return (Json(tokenObj.Item2));
                //}
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return (Json("Failed! Upload file not found."));
            }
        }
        [HttpPost]
        [Route("/blobstorage/uploadb64")]
        public async Task<JsonResult> AzureStorageUploadB64([FromBody] FileUploadInputObj inputObj)
        {
            //string userAccessToken = UtilityHelper.GetTokenFromHeaders(Request);
            //if (string.IsNullOrWhiteSpace(userAccessToken))
            //{
            //    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            //    return Json("Failed! Autherization header is missing.");
            //}
            //else 
            //if (string.IsNullOrWhiteSpace(inputObj.TeamId))
            //{
            //    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            //    return Json("Failed! TeamId is missing.");
            //}
            //else 
            if (string.IsNullOrWhiteSpace(inputObj.ChannelId))
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return Json("Failed! ChannelId is missing.");
            }
            else if (!string.IsNullOrEmpty(inputObj.base64))
            {
                //string spResourceUri = SharePointConstants.SharePointsiteUrl;
                //Tuple<bool, string> tokenObj = await TokenHelper.GetAccessToken_FromSSO(userAccessToken, spResourceUri);

                //if (tokenObj.Item1)
                //{
                //using (var ms = new MemoryStream())
                //{
                //    inputObj.file.CopyTo(ms);
                    byte[] fileBytes = Convert.FromBase64String(inputObj.base64);
                    string fileName = string.Format("Capture{0:YYYYMMddHHmm}{1}", DateTime.Now, Guid.NewGuid());
                    AzurestorageHelper objAzureStorage = new AzurestorageHelper();
                    string result = await objAzureStorage.UploadFromBinaryDataAsync($"{inputObj.TeamId}/{inputObj.ChannelId}/{fileName}", fileBytes);
                    if (!string.IsNullOrEmpty(result))
                    {
                        Response.StatusCode = (int)HttpStatusCode.OK;
                        inputObj.Name = fileName;
                        await AzureSearchHelper.AddAzureSearchIndex(inputObj, result);
                    }
                    else Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return Json(result);
                //}
                //}
                //else
                //{
                //    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                //    return (Json(tokenObj.Item2));
                //}
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return (Json("Failed! Upload file not found."));
            }
        }

        [HttpPost]
        [Route("/blobstorage/fileList")]
        public async Task<JsonResult> AzureStorageFileList([FromForm] FileUploadInputObj inputObj)
        {
            //string userAccessToken = UtilityHelper.GetTokenFromHeaders(Request);
            //if (string.IsNullOrWhiteSpace(userAccessToken))
            //{
            //    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            //    return Json("Failed! Autherization header is missing.");
            //}
            //else 
            if (string.IsNullOrWhiteSpace(inputObj.TeamId) && string.IsNullOrWhiteSpace(inputObj.ChannelId))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Failed! Pass TeamId or ChannelId.");
            }
            else
            {
                
                Tuple<bool, JObject> value = AzureSearchHelper.GetAzureSearchIndex(inputObj);
                Response.StatusCode = (int)HttpStatusCode.OK;
                return Json(value.Item2);

            }
        }
    }
}
