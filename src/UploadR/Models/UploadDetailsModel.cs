using System;
using System.Text.Json.Serialization;

namespace UploadR.Models
{
    public class UploadDetailsModel
    {
        /// <summary>
        ///     Id of the upload.
        /// </summary>
        public Guid UploadGuid { get; set; }
        
        /// <summary>
        ///     Id of the author of the upload.
        /// </summary>
        public Guid AuthorGuid { get; set; }
        
        /// <summary>
        ///     Date time this upload was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        ///     Last date time this upload was seen.
        /// </summary>
        public DateTime LastSeen { get; set; }
        
        /// <summary>
        ///     Amount of time this upload was seen.
        /// </summary>
        public long SeenCount { get; set; }
        
        /// <summary>
        ///     Name of the file.
        /// </summary>
        public string FileName { get; set; }
        
        /// <summary>
        ///     Content type of the file.
        /// </summary>
        public string ContentType { get; set; }
        
        /// <summary>
        ///     Whether this upload requires a password to be seen.
        /// </summary>
        public bool HasPassword { get; set; }
        
        /// <summary>
        ///     Gets the time this file expires in.
        /// </summary>
        [JsonIgnore]
        public TimeSpan ExpireAfter { get; set; }
        
        /// <summary>
        ///     Gets the time in milliseconds this file expires in.
        /// </summary>
        public int ExpireAfterMilliseconds => (int)ExpireAfter.TotalMilliseconds;
    }
}