using System;

namespace UploadR.Database.Models
{
    public sealed class Upload
    {
        /// <summary>
        ///     Unique ID of that upload.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        ///     User ID that uploaded this file.
        /// </summary>
        public Guid AuthorGuid { get; set; }

        /// <summary>
        ///     Date time this file was uploaded.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        ///     Last date time this file was seen.
        /// </summary>
        public DateTime LastSeen { get; set; }

        /// <summary>
        ///     Amount of time this file was seen.
        /// </summary>
        public long DownloadCount { get; set; }

        /// <summary>
        ///     Whether this file is removed or not.
        /// </summary>
        public bool Removed { get; set; }

        /// <summary>
        ///     Name of the file.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        ///     Type of the file.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        ///     Password of the file, if set.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        ///     User that uploaded this file.
        /// </summary>
        public User Author { get; set; }
    }
}
