namespace UploadR.Configurations
{
    public class OneTimeTokenConfiguration
    {
        public int Timeout { get; set; }

        public OneTimeTokenAntiSpanConfiguration AntiSpam { get; set; }
    }

    public class OneTimeTokenAntiSpanConfiguration
    {
        public bool Enabled { get; set; }

        public long MaxTry { get; set; }

        public int Timeout { get; set; }
    }
}
