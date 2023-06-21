using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;

namespace TeamsTabSSO.Helpers
{
    public class UtilityHelper
    {
        private static TelemetryClient? _telemetryClient;
        public UtilityHelper()
        {
            TelemetryConfiguration telemetryConfiguration= new TelemetryConfiguration();
            telemetryConfiguration.InstrumentationKey = Constants.Constants.TelemetryInstrumentationKey;
            telemetryConfiguration.ConnectionString = Constants.Constants.TelemetryConnectionString;
            _telemetryClient = new TelemetryClient(telemetryConfiguration);
        }

        public static string GetTokenFromHeaders(HttpRequest request)
        {
            string userAccessToken = string.Empty;
            try
            {
                var headers = request.Headers;
                if (headers.Any(x => x.Key == "Authorization"))
                {
                    userAccessToken = headers.FirstOrDefault(x => x.Key == "Authorization").Value.FirstOrDefault();
                    userAccessToken = userAccessToken.Replace("bearer", "").Replace("Bearer", "");
                }
                return userAccessToken.Trim();
            }
            catch
            {
                throw;
            }
        }
        public static JObject ValidJObject(string stringData)
        {
            try
            {
                return JObject.Parse(stringData);
            }
            catch
            {
                return null;
            }
        }
        public static void LogMessageInTxtFile(string logText)
        {
            try
            {
                _telemetryClient?.TrackTrace(logText);
                //string logFileName = "HttpsCalls_" + DateTime.Now.Date.ToString("MMMddyyyy") + ".txt";
                ////string strLogFilePath = $"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\\Files\\LogTexts\\{logFileName}";
                //string strLogFilePath = $"{AppDomain.CurrentDomain.GetData("ContentRootPath")}\\Files\\LogTexts\\{logFileName}";
                //using (StreamWriter log = new StreamWriter(strLogFilePath, true))
                //{
                //    // Write to the file:
                //    log.WriteLine($"{System.Environment.NewLine}{System.Environment.NewLine}{System.Environment.NewLine}{DateTime.Now}: {logText}");
                //    // Close the stream:
                //    log.Close();
                //}
            }
            catch
            {

            }
        }

        public static string GetUserNameFromToken(string token)
        {//preferred_username
            try
            {
                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
                var jti = jsonToken?.Claims.First(claim => claim.Type == "preferred_username").Value;
                return jti ?? "empty";
            }
            catch(Exception ex)
            {
                return "Ex:"+ex.Message;
            }
        }
    }
}
