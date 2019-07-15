using System;

namespace ShareY.Database.Models
{
    public sealed class Upload
    {
        public string Id { get; set; }
        public string AuthorId { get; set; }
        public DateTime CreatedAt { get; set; }
        public long DownloadCount { get; set; }
        public bool Visible { get; set; }
        public bool Removed { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }

        public User Author { get; set; }
    }
}
