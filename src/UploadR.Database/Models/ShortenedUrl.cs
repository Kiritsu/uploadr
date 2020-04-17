namespace UploadR.Database.Models
{
    public class ShortenedUrl : EntityBase
    {
        public string Url { get; set; }
        public string Shorten { get; set; }
    }
}