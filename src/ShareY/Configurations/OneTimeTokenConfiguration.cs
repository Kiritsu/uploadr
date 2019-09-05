using System;
using Newtonsoft.Json;

namespace ShareY.Configurations
{
    public class OneTimeTokenConfiguration
    {
        public bool Enabled { get; set; }

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
