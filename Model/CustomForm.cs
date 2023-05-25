namespace Microsoft.BotBuilderSamples.Models
{
    public class CustomFormResponse
    {
        public string EmpId { get; set; }
        public string EmpName { get; set; }
        public string EmpEmail { get; set; }
    }

    public class UploadFormResponse
    {
        public string photoName { get; set; }
        public string photoFileName { get; set; }
        public string photoFile { get; set; }
    }
}