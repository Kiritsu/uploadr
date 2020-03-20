using System;

namespace UploadR.Models
{
    public class UploadDetailsModel
    {
        public Guid UploadGuid { get; set; }
        public Guid AuthorGuid { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastSeen { get; set; }
        public long DownloadCount { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
    }
}