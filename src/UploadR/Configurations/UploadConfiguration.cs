namespace UploadR.Configurations
{
    public class UploadConfiguration
    {
        public long SizeMax { get; set; }
        public long SizeMin { get; set; }
        public string[] EnabledTypes { get; set; }
        public string UploadsPath { get; set; }
    }
}