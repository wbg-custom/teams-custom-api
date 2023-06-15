using Microsoft.Graph;
using Newtonsoft.Json;
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


    public class ResponseMessageCls
    {
        public string messsage { get; set; }
    }
    public class MetaDataFields
    {
        public string FieldName { get; set; }
        public object FieldValue { get; set; }
        public string FieldContext { get; set; }
    }
    public class MetaDataInput
    {
        public List<MetaDataFields> Fields { get; set; }
    }

    public class TeamPhotosIndexUploadFile
    {
        public List<TeamPhotosIndexFile> value { get; set; }
    }
    public class TeamPhotosIndexFile
    {
        public string id { get; set; }
        [JsonProperty(PropertyName = "@search.action")]
        public string action { get; set; }
        public string TeamId { get; set; }
        public string ChannelId { get; set; }
        public string? ItemId { get; set; }
        public string Name { get; set; }
        public string? CreatedBy { get; set; }
        public List<string>? tags { get; set; }
        public string fileUrl { get; set; }
    }
    public class TeamPhotosIndexSearch
    {
        public string search { get; set; }
        public string? searchFields { get; set; }
        public string? facets { get; set; }
        public string? filter { get; set; }
        public string? orderby { get; set; }
        public int? skip { get; set; }
        public int? top { get; set; }
    }
}
