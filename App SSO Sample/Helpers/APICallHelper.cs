using System.Net;
using System.Text;

namespace TeamsTabSSO.Helpers
{
    public class APICallHelper
    {
        //public static string MakeApiUploadFileRequest(string requestUrl, string accessToken, string strFileUrl)
        //{
        //    byte[] byteArray = File.ReadAllBytes(strFileUrl);
        //    string output = MakeApiUploadFileRequest(requestUrl, accessToken, byteArray);
        //    return output;
        //}
        public static string MakeApiUploadFileRequest(string requestUrl, string accessToken, byte[] byteArray)
        {
            string output = string.Empty;
            StreamWriter requestWriter = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUrl);
                request.Method = "POST";
                request.ContentType = "application/octet-stream";
                request.Accept = "application/json;odata=verbose";
                request.ContentLength = byteArray.Length;
                request.Headers.Add("Authorization", "Bearer " + accessToken);
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    output = reader.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                UtilityHelper.LogMessageInTxtFile($"url: {requestUrl} {Environment.NewLine}# ex.Message: {ex.Message} {Environment.NewLine}# ex.Response: {ex.Response} {Environment.NewLine}# ex.InnerException: {ex.InnerException} {Environment.NewLine}# response: {output}");
                if (ex.Response != null)
                {
                    var errorResponse = (HttpWebResponse)ex.Response;
                    if (errorResponse.StatusCode == HttpStatusCode.BadRequest)
                    {
                        StreamReader reader = new StreamReader(errorResponse.GetResponseStream());
                        string responseFromServer = reader.ReadToEnd();
                        //CrmErrorResponse error = JsonConvert.DeserializeObject<CrmErrorResponse>(responseFromServer);
                        throw new Exception(responseFromServer);
                    }
                }
                throw ex;
            }
            catch (Exception ex)
            {
                UtilityHelper.LogMessageInTxtFile($"url: {requestUrl} {Environment.NewLine}# ex.Message: {ex.Message} {Environment.NewLine}# ex.InnerException: {ex.InnerException} {Environment.NewLine}# response: {output}");
                throw;
            }
            finally
            {
                if (requestWriter != null)
                {
                    requestWriter.Close();
                }
            }
            return output;
        }
    }
}
