using System;

namespace ShareY.Models
{
    public class BaseViewModel
    {
        public bool IsAuthenticated { get; set; }
        public Guid UserToken { get; set; }
    }
}
