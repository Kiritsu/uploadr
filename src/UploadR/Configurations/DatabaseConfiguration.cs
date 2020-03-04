using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UploadR.Database;

namespace UploadR.Configurations
{
    public class DatabaseConfiguration : IDatabaseConfiguration
    {
        public string Hostname { get; set; }
        public int Port { get; set; }
        public string Database { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
