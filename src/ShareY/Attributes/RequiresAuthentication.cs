using System;

namespace ShareY.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequiresAuthentication : Attribute
    {
        public RequiresAuthentication()
        {

        }
    }
}
