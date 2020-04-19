using System;
using System.Text.Json.Serialization;

namespace UploadR.Models
{
    public class ShortenDetailsModel
    {
        /// <summary>
        ///     Id of the shortened url.
        /// </summary>
        public Guid ShortenedGuid { get; set; }
        
        /// <summary>
        ///     Id of the author.
        /// </summary>
        public Guid AuthorGuid { get; set; }
        
        /// <summary>
        ///     Url shortened.
        /// </summary>
        public string ShortenedUrl { get; set; }
        
        /// <summary>
        ///     Date time this shortened url was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        ///     Last date time this shortened url was seen. 
        /// </summary>
        public DateTime LastSeen { get; set; }
        
        /// <summary>
        ///     Amount of time this shortened url was seen.
        /// </summary>
        public long SeenCount { get; set; }
        
        /// <summary>
        ///     Whether this shortened url requires a password.
        /// </summary>
        public bool HasPassword { get; set; }
        
        /// <summary>
        ///     Amount of time from the creation time after which the shortened url expires. 
        /// </summary>
        [JsonIgnore]
        public TimeSpan ExpireAfter { get; set; }
        
        /// <summary>
        ///     Amount of milliseconds from the creation time after which the shortened url expires.
        /// </summary>
        public int ExpireAfterMilliseconds => (int)ExpireAfter.TotalMilliseconds;
    }
}