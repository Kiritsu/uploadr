namespace UploadR.Database.Models
{
    public sealed class Upload : EntityBase
    {
        /// <summary>
        ///     Name of the upload.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        ///     Type of the upload.
        /// </summary>
        public string ContentType { get; set; }
    }
}
