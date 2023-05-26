using Microsoft.Identity.Client;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System;

namespace TeamsMessagingExtensionsSearchAuthConfig.Helpers
{
    public class TokenHelper
    {
        public static Tuple<bool, string> GetAccessToken_MSAL(string authHeader, string audienceUri)
        {
            Tuple<bool, string> objResult;

            try
            {
                authHeader = authHeader.Replace("Bearer ", "");
                string clientId = Constants.AzureCredentials.AadClientID ?? "";
                string clientSecret = Constants.AzureCredentials.AadClientSecret ?? "";
                string aadInstance = Constants.AzureCredentials.AadInstance ?? "";
                string tenantId = Constants.AzureCredentials.AadTenantId ?? "";
                string authority = string.Format(CultureInfo.InvariantCulture, aadInstance, tenantId);

                IConfidentialClientApplication app = Microsoft.Identity.Client.ConfidentialClientApplicationBuilder.Create(clientId).WithClientSecret(clientSecret).WithAuthority(authority).Build();
                var userAssertion = new Microsoft.Identity.Client.UserAssertion(authHeader);
                var authResult = app.AcquireTokenOnBehalfOf(new string[] { $"{audienceUri}/.default" }, userAssertion).ExecuteAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                objResult = new Tuple<bool, string>(true, authResult.AccessToken);
            }
            catch (Exception ex)
            {
                objResult = new Tuple<bool, string>(false, $"Msg:{ex.Message}, StackTrace:{ex.StackTrace}");
            }

            return objResult;
        }

        public static async Task<Tuple<bool, string>> GetAccessToken_FromSSO(string ssoToken, string audienceUri)
        {
            Tuple<bool, string> objResult;

            try
            {
                ssoToken = ssoToken.Replace("Bearer ", "");
                ssoToken = ssoToken.Replace("'", "");
                string clientId = Constants.AzureCredentials.AadClientID ?? "";
                string clientSecret = Constants.AzureCredentials.AadClientSecret ?? "";
                string aadInstance = Constants.AzureCredentials.AadInstance ?? "";
                string tenantId = Constants.AzureCredentials.AadTenantId ?? "";
                string authority = string.Format(CultureInfo.InvariantCulture, aadInstance, tenantId);

                IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(clientId)
                                                .WithClientSecret(clientSecret)
                                                .WithAuthority(authority)
                                                .Build();

                UserAssertion assert = new UserAssertion(ssoToken);
                List<string> scopes = new List<string>();
                scopes.Add("https://graph.microsoft.com/User.Read");
                // Acquires an access token for this application (usually a Web API) from the authority configured in the application.
                var responseToken = await app.AcquireTokenOnBehalfOf(scopes, assert).ExecuteAsync();

                objResult = new Tuple<bool, string>(true, responseToken.AccessToken);


            }
            catch (Exception ex)
            {
                objResult = new Tuple<bool, string>(false, $"Msg:{ex.Message}, StackTrace:{ex.StackTrace}");
            }

            return objResult;
        }
    }
}
