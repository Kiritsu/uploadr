using System;
using ShareY.Database.Models;

namespace ShareY.Models
{
    public class BaseViewModel
    {
        public bool IsAdmin { get; set; }
        public bool IsAuthenticated { get; set; }
        public Guid? UserToken { get; set; }
    }
}
