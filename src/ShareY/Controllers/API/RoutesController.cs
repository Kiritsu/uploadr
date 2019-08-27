using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareY.Configurations;
using ShareY.Interfaces;

namespace ShareY.Controllers
{
    [Route("api/[controller]"), ApiController]
    public class RoutesController : Controller
    {
        private readonly RoutesConfiguration _routesConfiguration;

        public RoutesController(IRoutesConfigurationProvider routesConfiguration)
        {
            _routesConfiguration = routesConfiguration.GetConfiguration();
        }

        [HttpPatch, Route("{routeName}"), Authorize(Roles = "Admin")]
        public IActionResult Signup(string routeName, bool state)
        {
            var properties = _routesConfiguration.GetType().GetProperties();

            var property = properties.FirstOrDefault(x => x.Name.Equals(routeName, StringComparison.OrdinalIgnoreCase));
            if (property == null)
            {
                return BadRequest(properties.Select(x => x.Name));
            }

            property.SetValue(_routesConfiguration, state);

            return Ok(state);
        }
    }
}
