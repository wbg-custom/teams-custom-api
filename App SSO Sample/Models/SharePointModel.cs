namespace TeamsAuthSSO.Models
{
    public class SharePointModel
    {
    }

    public class FileUploadInputObj
    {
        public string TeamId { get; set; }
        public string ChannelId { get; set; }
        public string? ItemId { get; set; }
        public string Name { get; set; }
        public string? CreatedBy { get; set; }
        public List<string>? tags { get; set; }
        public IFormFile? file { get; set; }
        public string? base64 { get; set; }
    }
}
