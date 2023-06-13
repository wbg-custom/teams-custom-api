using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Newtonsoft.Json.Linq;

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
    }
}
