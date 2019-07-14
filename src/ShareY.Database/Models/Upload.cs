using System;
using ShareY.Database.Enums;

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
        public UploadType UploadType { get; set; }
        public byte[] Content { get; set; }

        public User Author { get; set; }
    }
}
