using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;
using TeamsAuthSSO.Models;
using TeamsTabSSO.Constants;

namespace TeamsTabSSO.Helpers
{
    public class SharePointHelper
    {
        public static HttpResponseObj UploadFileSharePoint(string accessToken, string folderName, string fileName, string strFilePath)
        {
            HttpResponseObj objReturn;

            string responseUrl = SharePointConstants.GetUploadFileUrl(folderName, fileName);
            string strResponseJson = APICallHelper.MakeApiUploadFileRequest(responseUrl, accessToken, strFilePath);

            objReturn = new HttpResponseObj(true, UtilityHelper.ValidJObject(strResponseJson), HttpStatusCode.OK);
            return objReturn;
        }
        public static HttpResponseObj UploadFileSharePoint(string accessToken, string folderName, string fileName, byte[] byteArray)
        {
            HttpResponseObj objReturn;

            string responseUrl = SharePointConstants.GetUploadFileUrl(folderName, fileName);
            string strResponseJson = APICallHelper.MakeApiUploadFileRequest(responseUrl, accessToken, byteArray);

            objReturn = new HttpResponseObj(true, UtilityHelper.ValidJObject(strResponseJson), HttpStatusCode.OK);
            return objReturn;
        }
        public static Task<Tuple<bool, JObject>> SharePointUploadAsync(string accessToken, string folderID, string fileName, string fileString)
        {
            Tuple<bool, JObject> objResult;

            string apiUrl = Constants.OneDriveConstants.OneDriveUploadFileUrl(folderID, fileName);

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
