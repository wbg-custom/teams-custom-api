using System;

namespace TeamsTabSSO.Constants
{
    public class Constants
    {
        /// <summary>
        /// Retry value in seconds
        /// </summary>
        public static int MsGraphApiDefaultRetryAfter { get { return 5; } }
        public static string TelemetryInstrumentationKey { get { return "c04bd50a-2395-4459-b351-7356dfc6b937"; } }
        public static string TelemetryConnectionString { get { return "InstrumentationKey=c04bd50a-2395-4459-b351-7356dfc6b937;IngestionEndpoint=https://eastus-8.in.applicationinsights.azure.com/;LiveEndpoint=https://eastus.livediagnostics.monitor.azure.com/"; } }
    }
    public static class AzureCredentials
    {
        public static string? AadClientID => Environment.GetEnvironmentVariable("AadClientID");
        public static string? AadClientSecret => Environment.GetEnvironmentVariable("AadClientSecret");
        public static string? AadTenantId => Environment.GetEnvironmentVariable("AadTenantId");
        public static string AadInstance { get { return "https://login.microsoftonline.com/{0}"; } }
        public static string Aad1Instance { get { return "https://login.microsoftonline.com/{0}/tokens/OAuth/2"; } }
        public static string AadSPInstance { get { return "https://accounts.accesscontrol.windows.net/{0}/tokens/OAuth/2"; } }
        public static string AadSP1Instance { get { return "https://accounts.accesscontrol.windows.net/{0}"; } }
        public static string AadSP2Instance { get { return "https://login.microsoftonline.com/wbgcustomoutlook.onmicrosoft.com/oauth2/v2.0/token"; } }
    }
    public static class AzureStorageConstants
    {
        public static string StorageAccountName { get { return "storageteamscustomapp"; } }
        public static string BlobContainerName { get { return "wbg-files"; } }
        public static string ConnectionString { get { return "DefaultEndpointsProtocol=https;AccountName=storageteamscustomapp;AccountKey=Pjw/hKRXyE21uuOpzcEBIhYsTFeuDzwhdt5gzwHDXRBS7JJRlEXfxqzLx3tzF5RDoYBAKY8MDion+AStwmvgMw==;EndpointSuffix=core.windows.net"; } }
    }
    public static class OneDriveConstants
    {
        private static string _siteUrl { get { return "https://graph.microsoft.com/v1.0/me"; } }
        public static string GetOneDriveFolderIDUrl()
        {
            return $"{_siteUrl}/drive/root/search(q='team photos')";
        }
        public static string GetOneDrivePhotoListUrl(string folderID)
        {
            return $"{_siteUrl}/drive/items/{folderID}/children?select=id,name,size,webUrl,@microsoft.graph.downloadUrl";
        }
        public static string OneDriveUploadFileUrl(string folderId, string fileName)
        {
            return $"{_siteUrl}/drive/items/{folderId}:/{fileName}:/content";
        }
    }
    public static class SharePointConstants
    {
        public static string SharePointsiteUrl { get { return "https://wbgcustomoutlook.sharepoint.com"; } }
        public static string FolderName { get { return "WBG Phtos"; } }
        public static string GetOneDriveFolderIDUrl()
        {
            return $"{SharePointsiteUrl}/drive/root/search(q='team photos')";
        }
        public static string GetDownloadFileUrl(string folderName, string fileName)
        {
            return $"{SharePointsiteUrl}/_api/web/GetFolderByServerRelativeUrl('{folderName}')/Files('{fileName}')/$value";
        }
        public static string GetUploadFileUrl(string folderName, string fileName)
        {
            return $"{SharePointsiteUrl}/_api/web/GetFolderByServerRelativeUrl('{folderName}')/Files/Add(url='{fileName}', overwrite=true)";
        }
    }
    public static class AzureIndexConstants
    {
        public static string AzureSearchPostUrl {  get { return "https://{0}.search.windows.net/indexes/{1}/docs/search?api-version=2020-06-30"; } }
        public static string AzureSearchPrefix { get { return "search-teams-custom-app"; } }
        public static string AzureSearchIndexName { get { return "team-photos-index"; } }
        public static string IndexUploadUrl { get { return " https://{0}.search.windows.net/indexes/{1}/docs/index?api-version=2020-06-30"; } }
        public static string apiKey { get { return "BerXHnJ4X3LgpeQ4ouTRHArwWP4CI2yigYrZEvmgmPAzSeB9qF5j"; } }
    }


    public enum HttpMethods
    {
        GET = 0,
        POST = 1,
        PUT = 2,
        PATCH = 3,
        DELETE = 4
    }
}
