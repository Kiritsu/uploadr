using System;

namespace UploadR.Models
{
    public class UploadModel
    {
        /// <summary>
        ///     Password to use to not let these files available for everyone.
        /// </summary>
        public string Password { get; set; }
        
        /// <summary>
        ///     Gets the duration of the files.
        /// </summary>
        public TimeSpan ExpireAfter { get; set; } = TimeSpan.Zero;
    }
}