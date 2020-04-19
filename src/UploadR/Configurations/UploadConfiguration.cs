namespace UploadR.Configurations
{
    public class UploadConfiguration : EntityConfiguration
    {
        public string[] EnabledTypes { get; set; }
        public string UploadsPath { get; set; }
    }
}