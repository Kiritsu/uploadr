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
        ///     Gets the proposal url. Will be randomized if already used.
        /// </summary>
        public string Proposal { get; set; }
        
        /// <summary>
        ///     Password to use to not let this shortened url available for everyone.
        /// </summary>
        public string Password { get; set; }
        
        /// <summary>
        ///     Gets the duration of the files.
        /// </summary>
        public TimeSpan ExpireAfter { get; set; } = TimeSpan.Zero;
    }
}