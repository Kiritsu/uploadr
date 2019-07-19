using System;
using ShareY.Database.Enums;

namespace ShareY.Database.Models
{
    public sealed class Token
    {
        public Guid Guid { get; set; }
        public Guid UserGuid { get; set; }
        public DateTime CreatedAt { get; set; }
        public User User { get; set; }
        public TokenType TokenType { get; set; }
    }
}
