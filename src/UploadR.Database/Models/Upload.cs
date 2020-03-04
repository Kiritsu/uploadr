using System;

namespace UploadR.Database.Models
{
    public sealed class Upload
    {
        /// <summary>
        ///     Id of the upload.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        ///     Id of the author's upload.
        /// </summary>
        public Guid AuthorGuid { get; set; }

        /// <summary>
        ///     Date time this upload was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        ///     Last date time this file was seen.
        /// </summary>
        public DateTime LastSeen { get; set; }

        /// <summary>
        ///     Amount of time this upload was seen.
        /// </summary>
        public long DownloadCount { get; set; }

        /// <summary>
        ///     Whether this upload is removed or not.
        /// </summary>
        public bool Removed { get; set; }

        /// <summary>
        ///     Name of the upload.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        ///     Type of the upload.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        ///     Password of the upload, if set.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        ///     Author of the upload.
        /// </summary>
        public User Author { get; set; }
    }
}
