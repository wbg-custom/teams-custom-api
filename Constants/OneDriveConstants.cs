namespace TeamsMessagingExtensionsSearchAuthConfig.Constants
{
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
