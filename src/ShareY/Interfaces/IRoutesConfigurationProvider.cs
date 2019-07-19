using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShareY.Configurations;

namespace ShareY.Interfaces
{
    public interface IRoutesConfigurationProvider
    {
        RoutesConfiguration GetConfiguration();
    }
}
