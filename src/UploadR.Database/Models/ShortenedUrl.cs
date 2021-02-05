namespace UploadR.Database.Models
{
    public class ShortenedUrl : BaseEntity
    {
        /// <summary>
        ///     Url to redirect to.
        /// </summary>
        public string Url { get; set; }
    }
}