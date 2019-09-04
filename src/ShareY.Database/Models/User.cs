using System;
using System.Collections.Generic;

namespace ShareY.Database.Models
{
    public sealed class User
    {
        public Guid Guid { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Email { get; set; }
        public bool Disabled { get; set; }
        public Token Token { get; set; }
        public ICollection<Upload> Uploads { get; set; }
    }
}
