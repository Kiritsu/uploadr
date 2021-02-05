using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace UploadR.Controllers
{
    [Route("[controller]")]
    public class ThemeController : UploadRController
    {
        [HttpPost]
        public IActionResult ToggleTheme()
        {
            var theme = Theme == "dark" ? "light" : "dark"; 
            
            ViewData["uploadr_theme"] = theme;
            Response.Cookies.Append("uploadr_theme", theme);

            return Ok();
        }
    }
}