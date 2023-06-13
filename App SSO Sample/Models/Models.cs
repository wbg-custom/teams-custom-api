using Microsoft.Graph;
using Newtonsoft.Json.Linq;
using System.Net;

namespace TeamsAuthSSO.Models
{
    public class UploadFormResponse
    {
        public string photoName { get; set; }
        public string photoFileName { get; set; }
        public string photoFile { get; set; }
    }
    public class HttpResponseObj
    {
        public HttpResponseObj()
        {
        }
        public HttpResponseObj(bool isSuccess, JObject resultJson, HttpStatusCode httpStatusCode)
        {
            this.isSuccess = isSuccess;
            this.resultJson = resultJson;
            this.httpStatusCode = httpStatusCode;
        }
        public HttpResponseObj(bool isSuccess, string resultStr, HttpStatusCode httpStatusCode)
        {
            this.isSuccess = isSuccess;
            this.resultStr = resultStr;
            this.httpStatusCode = httpStatusCode;
        }
        public bool isSuccess { get; set; }
        public JObject resultJson { get; set; }
        public string resultStr { get; set; }
        public HttpStatusCode httpStatusCode { get; set; }
    }



    public class MetaDataFields
    {
        public string FieldName { get; set; }
        public object FieldValue { get; set; }
        public string FieldContext { get; set; }
    }
    public class MetaDataInput
    {
        public List<MetaDataFields> Fields { get; set;}
    }

}
