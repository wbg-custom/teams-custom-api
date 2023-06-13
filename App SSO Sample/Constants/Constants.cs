﻿using System;

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
        private static string _siteUrl { get { return "https://wbgcustomoutlook.sharepoint.com"; } }
        public static string GetDownloadFileUrl(string folderName, string fileName)
        {
            return $"{_siteUrl}/_api/web/GetFolderByServerRelativeUrl('{folderName}')/Files('{fileName}')/$value";
        }
        public static string GetUploadFileUrl(string folderName, string fileName)
        {
            return $"{_siteUrl}/_api/web/GetFolderByServerRelativeUrl('{folderName}')/Files/Add(url='{fileName}', overwrite=true)";
        }
    }
    public static class AzureSearchConstants
    {
        public static string AzureSearchUrl {  get { return "https://{0}.search.windows.net/indexes/{1}/docs?api-version=2020-06-30"; } }
        public static string AzureSearchPrefix { get { return "search-teams-custom-app"; } }
        public static string AzureSearchIndexName { get { return "team-photos-index"; } }
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
