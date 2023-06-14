namespace TeamsAuthSSO.Models
{
    public class SharePointModel
    {
    }

    public class SharePointUploadInputObj
    {
        public string TeamId { get; set; }
        public string ChannelId { get; set; }
        public string ItemId { get; set; }
        public string Name { get; set; }
        public string CreatedBy { get; set; }
        public string tags { get; set; }
        public IFormFile file { get; set; }
    }
}
