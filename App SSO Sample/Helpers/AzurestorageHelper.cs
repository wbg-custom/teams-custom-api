using Azure.Identity;
using Azure.Storage;
using Azure.Storage.Blobs;
using System;
using TeamsTabSSO.Constants;

namespace App_SSO_Sample.Helpers
{
    public class AzurestorageHelper
    {
        private BlobContainerClient _blobContainerClient;
        public AzurestorageHelper()
        {
            //Uri serviceUri = new Uri($"https://{AzureStorageContants.StorageAccountName}.blob.core.windows.net");
            //BlobServiceClient _blobServiceClient = new BlobServiceClient(serviceUri, new DefaultAzureCredential());
            BlobServiceClient _blobServiceClient = new BlobServiceClient(AzureStorageConstants.ConnectionString);
            _blobContainerClient = _blobServiceClient.GetBlobContainerClient(AzureStorageConstants.BlobContainerName);
        }

        public async Task<string> UploadFromBinaryDataAsync(
           string fileName, byte[] fileBytes)
        {
            //string fileName = Path.GetFileName(localFilePath);
            BlobClient blobClient = _blobContainerClient.GetBlobClient(fileName);

            //FileStream fileStream = File.OpenRead(localFilePath);
            //BinaryReader reader = new BinaryReader(fileStream);

            //byte[] buffer = new byte[fileStream.Length];
            //reader.Read(buffer, 0, buffer.Length);
            //BinaryData binaryData = new BinaryData(buffer);

            BinaryData binaryData = new BinaryData(fileBytes);
            await blobClient.UploadAsync(binaryData, true);

            //fileStream.Close();
            return blobClient.Uri.ToString();
        }
    }
}
