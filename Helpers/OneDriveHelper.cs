//using Microsoft.Graph;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using TeamsMessagingExtensionsSearchAuthConfig.Constants;
using System.Net;
using System.IO;
using System.Text;

namespace TeamsMessagingExtensionsSearchAuthConfig.helper
{
    public static class OneDriveHelper
    {
        public static async Task<Tuple<bool, string>> GetOneDriveFolderIDAsync(string accessToken)
        {
            Tuple<bool, string> objResult;
            HttpClient objClient = new HttpClient();
            objClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage objResponse = await objClient.GetAsync(OneDriveConstants.GetOneDriveFolderIDUrl());
            if (objResponse.IsSuccessStatusCode)
            {
                var jsonObj = JObject.Parse(await objResponse.Content.ReadAsStringAsync());
                objResult = new Tuple<bool, string>(true, "" + jsonObj["value"][0]["id"]);
            }
            else
            {
                objResult = new Tuple<bool, string>(false, $"Message:{objResponse.StatusCode}");
            }
            return objResult;
        }
        public static async Task<Tuple<bool, JObject>> GetOneDrivePhotoListAsync(string accessToken, string folderID)
        {
            Tuple<bool, JObject> objResult;
            HttpClient objClient = new HttpClient();
            objClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage objResponse = await objClient.GetAsync(OneDriveConstants.GetOneDrivePhotoListUrl(folderID));
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
        public static Task<Tuple<bool, JObject>> UploadOneDrivePhotoAsync(string accessToken, string folderID, string fileName, string fileString)
        {
            Tuple<bool, JObject> objResult;

            string apiUrl = OneDriveConstants.OneDriveUploadFileUrl(folderID, fileName);

            var b64 = fileString.Split("base64,")[1];
            byte[] byteArray = Convert.FromBase64String(b64);
            WebRequest request = WebRequest.Create(apiUrl);
            request.Method = "PUT";
            request.ContentType = "application/octet-stream";
            request.ContentLength = byteArray.Length;
            request.Headers.Add("Content-Disposition", $"filename*=UTF-8''{fileName}");
            request.Headers.Add("Authorization", $"Bearer {accessToken}");
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            //((HttpWebResponse)response).StatusDescription.Dump();

            if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK)
            {
                string stringData;
                var encoding = ASCIIEncoding.ASCII;
                using (var reader = new StreamReader(response.GetResponseStream(), encoding))
                {
                    stringData = reader.ReadToEnd();
                }
                objResult = new Tuple<bool, JObject>(true, JObject.Parse(stringData));
            }
            else
            {
                objResult = new Tuple<bool, JObject>(false, JObject.Parse($"Something went wrong. Please try after sometime. StatusCode:{response.StatusCode}. StatusDescription:{response.StatusDescription}."));
            }

            return Task.FromResult(objResult);
        }
    }
}
