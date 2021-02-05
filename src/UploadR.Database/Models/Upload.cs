namespace UploadR.Database.Models
{
    public sealed class Upload : BaseEntity
    {
        /// <summary>
        ///     Type of the upload.
        /// </summary>
        public string ContentType { get; set; }
    }
}
