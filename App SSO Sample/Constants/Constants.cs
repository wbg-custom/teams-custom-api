using System;

namespace TeamsTabSSO.Constants
{
    public class Constants
    {
        /// <summary>
        /// Retry value in seconds
        /// </summary>
        public static int MsGraphApiDefaultRetryAfter { get { return 5; } }
    }
    public static class AzureCredentials
    {
        public static string? AadClientID => Environment.GetEnvironmentVariable("AadClientID");
        public static string? AadClientSecret => Environment.GetEnvironmentVariable("AadClientSecret");
        public static string? AadTenantId => Environment.GetEnvironmentVariable("AadTenantId");
        public static string AadInstance { get { return "https://login.microsoftonline.com/{0}"; } }
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

    public enum HttpMethods
    {
        GET = 0,
        POST = 1,
        PUT = 2,
        PATCH = 3,
        DELETE = 4
    }
}
