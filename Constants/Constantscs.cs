using System;

namespace TeamsMessagingExtensionsSearchAuthConfig.Constants
{
    public class Constantscs
    {
    }
    public class AzureCredentials
    {
        public static string? AadClientID { get { return Environment.GetEnvironmentVariable("AadClientID"); } }
        public static string? AadClientSecret { get { return Environment.GetEnvironmentVariable("AadClientSecret"); } }
        public static string? AadTenantId { get { return Environment.GetEnvironmentVariable("AadTenantId"); } }
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
}
