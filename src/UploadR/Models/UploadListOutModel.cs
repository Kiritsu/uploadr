using UploadR.Enums;

namespace UploadR.Models
{
    public class UploadListOutModel
    {
        /// <summary>
        ///     Uploads that failed.
        /// </summary>
        public UploadOutModel[] FailedUploads { get; set; }
        
        /// <summary>
        ///     Uploads that succeeded.
        /// </summary>
        public UploadOutModel[] SucceededUploads { get; set; }
    }

    public class UploadOutModel
    {
        /// <summary>
        ///     Name given to the file. 
        /// </summary>
        public string Filename { get; set; }
        
        /// <summary>
        ///     Type of the file.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        ///     Size of the file in bytes.
        /// </summary>
        public long Size { get; set; }
        
        /// <summary>
        ///     Whether a password is needed to see that file.
        /// </summary>
        public bool HasPassword { get; set; }
        
        /// <summary>
        ///     Status code of the file.
        /// </summary>
        public UploadStatusCode StatusCode { get; set; }
    }
}