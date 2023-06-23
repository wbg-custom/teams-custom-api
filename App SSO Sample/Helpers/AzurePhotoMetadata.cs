
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace TeamsTabSSO.Helpers
{
    public class AzurePhotoMetadata
    {
        public static async Task<List<string>> ImageMetadata(string imageUrl)
        {
            List<string> returnLst = new List<string>();//AzureCredentials
            // Create a client
            ComputerVisionClient client = Authenticate(Constants.AzureCredentials.AzurePhotoMetadataEndPoint, Constants.AzureCredentials.AzurePhotoMetadataKey);

            // Analyze an image to get features and other properties.
            List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
            {
                VisualFeatureTypes.Tags
            };

            // Analyze the URL image 
            ImageAnalysis results = await client.AnalyzeImageAsync(imageUrl, visualFeatures: features);
            // Image tags and their confidence score
            foreach (var tag in results.Tags)
            {
                //Console.WriteLine($"{tag.Name} {tag.Confidence}");
                if (!returnLst.Contains(tag.Name))
                {
                    returnLst.Add(tag.Name);
                }
            }

            return returnLst;
        }
        //public static async Task<List<string>> ImageMetadata(byte[] imgBytes)
        //{
        //    List<string> returnLst = new List<string>();//AzureCredentials
        //    // Create a client
        //    ComputerVisionClient client = Authenticate(Constants.AzureCredentials.AzurePhotoMetadataEndPoint, Constants.AzureCredentials.AzurePhotoMetadataKey);

        //    // Analyze an image to get features and other properties.
        //    List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
        //    {
        //        VisualFeatureTypes.Tags
        //    };

        //    using (MemoryStream stream = new MemoryStream(imgBytes))
        //    {
        //        ImageAnalysis results = await client.AnalyzeImageInStreamAsync(stream, visualFeatures: features);
        //        foreach (var tag in results.Tags)
        //        {
        //            if (!returnLst.Contains(tag.Name))
        //            {
        //                returnLst.Add($"{tag.Name}:{tag.Confidence}");
        //            }
        //        }
        //    }

        //    return returnLst;
        //}
        
        public static ComputerVisionClient Authenticate(string endpoint, string key)
        {
            ComputerVisionClient client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(key)) { Endpoint = endpoint };
            return client;
        }
    }
}
