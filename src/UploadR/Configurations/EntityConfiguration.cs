using System;

namespace UploadR.Configurations
{
    public class EntityConfiguration
    {
        public int SizeMax { get; set; }
        public int SizeMin { get; set; }
        public TimeSpan DefaultExpiry { get; set; }
        public int BulkLimit { get; set; }
    }
}