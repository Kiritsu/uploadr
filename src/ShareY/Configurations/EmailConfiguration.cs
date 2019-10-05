namespace ShareY.Configurations
{
    public class EmailConfiguration 
    {
        public string Server { get; set; }
        public string Sender { get; set; }
        public int Port { get; set; }
        public bool UseSsl { get; set; }

        public string Auth { get; set; }
        public string Password { get; set; }

        public int Timeout { get; set; }
    }
}
