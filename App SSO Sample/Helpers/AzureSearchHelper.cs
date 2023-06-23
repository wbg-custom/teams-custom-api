using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Net;
using System.Text.Json.Serialization;
using TeamsAuthSSO.Models;
using TeamsTabSSO.Constants;
using TeamsTabSSO.Helpers;

namespace App_SSO_Sample.Helpers
{
    public class AzureSearchHelper
    {
        public static async Task<Tuple<bool, string>> AddAzureSearchIndex(FileUploadInputObj objInputData, string fileUrl)
        {
            if(objInputData.tags == null || objInputData.tags.Count == 0)
            {
                string fileExtension = Path.GetExtension(fileUrl);
                if (!string.IsNullOrWhiteSpace(fileExtension))
                {
                    fileExtension = fileExtension.ToLower().Replace(".", "");
                    if(fileExtension == "jpeg" || fileExtension == "jpg" || fileExtension == "png")
                    {
                        objInputData.tags = await AzurePhotoMetadata.ImageMetada(fileUrl);
                    }
                }
            }

            TeamPhotosIndexFile objIndexFile = new TeamPhotosIndexFile()
            {
                id= Guid.NewGuid().ToString(),
                action= "upload",
                ChannelId= objInputData.ChannelId,
                CreatedBy= objInputData.CreatedBy?? "",
                fileUrl = fileUrl,
                ItemId= objInputData.ItemId?? "",
                Name= objInputData.Name,
                tags = objInputData.tags?? new List<string>(),
                TeamId = objInputData.TeamId
            };
            TeamPhotosIndexUploadFile objUploadData = new TeamPhotosIndexUploadFile();
            objUploadData.value = new List<TeamPhotosIndexFile>
            {
                objIndexFile
            };
            string jsonValue = JsonConvert.SerializeObject(objUploadData);
            //StringContent conetnData = new StringContent(jsonValue, System.Text.Encoding.UTF8, "application/json");

            string uploadUrl = string.Format(CultureInfo.InvariantCulture, AzureIndexConstants.IndexUploadUrl, AzureIndexConstants.AzureSearchPrefix, AzureIndexConstants.AzureSearchIndexName);
            HttpClient objClient = new HttpClient();
            objClient.DefaultRequestHeaders.Accept.Clear();
            objClient.DefaultRequestHeaders.Add("accept", "application/json");
            objClient.DefaultRequestHeaders.Add("api-key", AzureIndexConstants.apiKey);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uploadUrl);
            request.Content = new StringContent(jsonValue, System.Text.Encoding.UTF8, "application/json");

            var response = objClient.SendAsync(request).GetAwaiter().GetResult();

            string responseData = response.Content.ReadAsStringAsync().Result;
            Tuple<bool, string> objReturn = new Tuple<bool, string>(true, responseData);
            return objReturn;
        }
        public static Tuple<bool, JObject> GetAzureSearchIndex(FileUploadInputObj objInputData)
        {
            TeamPhotosIndexSearch objSearchData = new TeamPhotosIndexSearch();
            //objSearchData.search = string.IsNullOrEmpty(objInputData.ChannelId) ? objInputData.TeamId: objInputData.ChannelId;
            //objSearchData.searchFields = string.IsNullOrEmpty(objInputData.ChannelId) ? "TeamId" : "ChannelId";
            objSearchData.search = "*";
            objSearchData.filter = string.IsNullOrEmpty(objInputData.ChannelId) ? $"TeamId eq '{objInputData.TeamId}'" : $"ChannelId eq '{objInputData.ChannelId}'";
            string jsonValue = JsonConvert.SerializeObject(objSearchData);
            //StringContent conetnData = new StringContent(jsonValue, System.Text.Encoding.UTF8, "application/json");

            string uploadUrl = string.Format(CultureInfo.InvariantCulture, AzureIndexConstants.AzureSearchPostUrl, AzureIndexConstants.AzureSearchPrefix, AzureIndexConstants.AzureSearchIndexName);
            HttpClient objClient = new HttpClient();
            objClient.DefaultRequestHeaders.Accept.Clear();
            objClient.DefaultRequestHeaders.Add("accept", "application/json");
            objClient.DefaultRequestHeaders.Add("api-key", AzureIndexConstants.apiKey);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uploadUrl);
            request.Content = new StringContent(jsonValue, System.Text.Encoding.UTF8, "application/json");

            var response = objClient.SendAsync(request).GetAwaiter().GetResult();

            string responseData = response.Content.ReadAsStringAsync().Result;
            JObject responseJson = UtilityHelper.ValidJObject(responseData);
            Tuple<bool, JObject> objReturn;
            if (responseJson != null)
            {
                objReturn = new Tuple<bool, JObject>(true, responseJson);
            }
            else
            {
                ResponseMessageCls objResponse= new ResponseMessageCls();
                objResponse.messsage = !string.IsNullOrEmpty(responseData) ? responseData : $"Failed! Statuscode: {response.StatusCode}";
                objReturn = new Tuple<bool, JObject>(false, JObject.FromObject(objResponse));
            }
            return objReturn;
        }
    }
}
