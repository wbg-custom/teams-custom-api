using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using TeamsAuthSSO.Models;
using TeamsTabSSO.Constants;

namespace TeamsTabSSO.Helpers
{
    public class SharePointHelper
    {
        //public static HttpResponseObj UploadFileSharePoint(string accessToken, string fileName, string strFilePath)
        //{
        //    HttpResponseObj objReturn;
        //    string folderName = Constants.SharePointConstants.FolderName;
        //    string responseUrl = SharePointConstants.GetUploadFileUrl(folderName, fileName);
        //    string strResponseJson = APICallHelper.MakeApiUploadFileRequest(responseUrl, accessToken, strFilePath);

        //    objReturn = new HttpResponseObj(true, UtilityHelper.ValidJObject(strResponseJson), HttpStatusCode.OK);
        //    return objReturn;
        //}
        public static HttpResponseObj UploadFileSharePoint(string accessToken, string fileName, byte[] byteArray)
        {
            string folderName = Constants.SharePointConstants.FolderName;
            string responseUrl = SharePointConstants.GetUploadFileUrl(folderName, fileName);
            string strResponseJson = APICallHelper.MakeApiUploadFileRequest(responseUrl, accessToken, byteArray);
            HttpResponseObj objReturn = new HttpResponseObj(true, UtilityHelper.ValidJObject(strResponseJson), HttpStatusCode.OK);
            return objReturn;
        }

        public static async Task<Tuple<bool, string>> GetSharePointFolderIDAsync(string accessToken)
        {
            Tuple<bool, string> objResult;
            HttpClient objClient = new HttpClient();
            objClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage objResponse = await objClient.GetAsync(Constants.OneDriveConstants.GetOneDriveFolderIDUrl());
            if (objResponse.IsSuccessStatusCode)
            {
                var jsonObj = JObject.Parse(await objResponse.Content.ReadAsStringAsync());
                objResult = new Tuple<bool, string>(true, "" + jsonObj?["value"]?[0]?["id"]);
            }
            else
            {
                objResult = new Tuple<bool, string>(false, $"Message:{objResponse.StatusCode}");
            }
            return objResult;
        }
        public static async Task<Tuple<bool, JObject>> GetSharePointPhotoListAsync(string accessToken, string folderID)
        {
            Tuple<bool, JObject> objResult;
            HttpClient objClient = new HttpClient();
            objClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage objResponse = await objClient.GetAsync(Constants.OneDriveConstants.GetOneDrivePhotoListUrl(folderID));
            if (objResponse.IsSuccessStatusCode)
            {
                var jsonObj = JObject.Parse(await objResponse.Content.ReadAsStringAsync());
                objResult = new Tuple<bool, JObject>(true, jsonObj);
            }
            else
            {
                objResult = new Tuple<bool, JObject>(false, new JObject($"Message:{objResponse.StatusCode}"));
            }
            return objResult;
        }

    }
}
