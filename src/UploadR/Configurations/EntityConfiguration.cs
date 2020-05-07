using System;

namespace UploadR.Configurations
{
    public class EntityConfiguration
    {
        public long SizeMax { get; set; }
        public long SizeMin { get; set; }
        public TimeSpan DefaultExpiry { get; set; }
        public long BulkLimit { get; set; }
    }
}