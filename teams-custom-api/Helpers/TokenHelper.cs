using System.Globalization;

namespace teams_custom_api.Helpers
{
    public class TokenHelper
    {
        public static Tuple<bool, string> GetAccessToken_MSAL(string authHeader, string audienceUri)
        {
            Tuple<bool, string> objResult;

            try
            {
                authHeader = authHeader.Replace("Bearer ", "");
                string clientId = "cc30ca9f-7ec1-4b9e-a2c1-3141740a0f93";
                string clientSecret = "DzJ8Q~w5I7fdL-QWdZbf6nMiQl1EjJYUgMmt3cQL";
                string aadInstance = "https://login.microsoftonline.com/{0}";
                string tenantId = "7f611e52-fc16-4e0c-b873-87b7705ddd43";
                string authority = string.Format(CultureInfo.InvariantCulture, aadInstance, tenantId);

                Microsoft.Identity.Client.IConfidentialClientApplication app = Microsoft.Identity.Client.ConfidentialClientApplicationBuilder.Create(clientId).WithClientSecret(clientSecret).WithAuthority(authority).Build();
                var userAssertion = new Microsoft.Identity.Client.UserAssertion(authHeader);
                var authResult = app.AcquireTokenOnBehalfOf(new string[] { $"{audienceUri}/.default" }, userAssertion ).ExecuteAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                objResult = new Tuple<bool, string>(true, authResult.AccessToken);
            }
            catch(Exception ex)
            {
                objResult = new Tuple<bool, string>(false, $"Msg:{ex.Message}, StackTrace:{ex.StackTrace}");
            }

            return objResult;
        }
    }
}
