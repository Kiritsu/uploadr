using System;

namespace UploadR.Database.Models
{
    public sealed class Upload
    {
        public Guid Guid { get; set; }
        public Guid AuthorGuid { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastSeen { get; set; }
        public long ViewCount { get; set; }
        public bool Removed { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }

        public User Author { get; set; }
    }
}
