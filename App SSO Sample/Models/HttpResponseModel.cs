
using Newtonsoft.Json.Linq;
using System.Net;

namespace TeamsTabSSO.Models
{
    public class HttpsHeaderObj
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
    public class HttpsInputObj
    {
        public string accessToken { get; set; }
        public string url { get; set; }
        public Constants.HttpMethods method { get; set; }
        public object postData { get; set; }
        public List<HttpsHeaderObj> httpHeaders { get; set; }
        public bool isResponseInByteArray { get; set; }
    }
    public class HttpsResponseObj
    {
        public HttpStatusCode StatusCode { get; set; }
        public string StringResponse { get; set; }
        public byte[] bytesResponse { get; set; }
        public JObject JsonResponse { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorStrackTrace { get; set; }
    }
}
