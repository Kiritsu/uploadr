using System;

namespace UploadR.Models
{
    public class ShortenModel
    {
        /// <summary>
        ///     Gets the url to shorten.
        /// </summary>
        public string Url { get; set; }
        
        /// <summary>
        ///     Gets the duration of the files.
        /// </summary>
        public TimeSpan ExpireAfter { get; set; } = TimeSpan.Zero;
    }
}