using System;
using System.Text.Json.Serialization;

namespace UploadR.Models
{
    public class ShortenOutModel
    {
        /// <summary>
        ///     Gets the url to shorten.
        /// </summary>
        public string BaseUrl { get; set; }
        
        /// <summary>
        ///     Gets the url to shorten.
        /// </summary>
        public string ShortenedUrl { get; set; }
        
        /// <summary>
        ///     Whether a password is needed to see that file.
        /// </summary>
        public bool HasPassword { get; set; }
        
        /// <summary>
        ///     Gets the time this file expire in.
        /// </summary>
        [JsonIgnore]
        public TimeSpan ExpireAfter { get; set; }

        public int ExpireAfterMilliseconds => (int)ExpireAfter.TotalMilliseconds;
    }
}