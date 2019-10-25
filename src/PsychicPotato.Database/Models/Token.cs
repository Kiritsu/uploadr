using System;
using PsychicPotato.Database.Enums;

namespace PsychicPotato.Database.Models
{
    public sealed class Token
    {
        public Guid Guid { get; set; }
        public Guid UserGuid { get; set; }
        public DateTime CreatedAt { get; set; }
        public TokenType TokenType { get; set; }
        public bool Revoked { get; set; }
        public User User { get; set; }
    }
}
