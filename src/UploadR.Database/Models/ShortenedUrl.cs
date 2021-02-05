namespace UploadR.Database.Models
{
    public class ShortenedUrl : EntityBase
    {
        /// <summary>
        ///     Url to redirect to.
        /// </summary>
        public string Url { get; set; }
        
        /// <summary>
        ///     Shorten url.
        /// </summary>
        public string Shorten { get; set; }
    }
}