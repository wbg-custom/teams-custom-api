namespace teams_custom_api.Constants
{
    public class Constants
    {

    }

    public class AzureCredentials 
    {
        public static string? AadClientID { get { return Environment.GetEnvironmentVariable("AadClientID"); } }
        public static string? AadClientSecret { get { return Environment.GetEnvironmentVariable("AadClientSecret"); } }
        public static string? AadTenantId { get { return Environment.GetEnvironmentVariable("AadTenantId"); } }
        public static string AadInstance { get { return "https://login.microsoftonline.com/{0}"; } }
    }

}
