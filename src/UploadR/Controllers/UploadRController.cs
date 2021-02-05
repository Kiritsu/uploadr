using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace UploadR.Controllers
{
    public class UploadRController : Controller
    {
        protected string Theme => Request.Cookies["uploadr_theme"] ?? "dark";
        protected Guid UserGuid => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            ViewData["uploadr_theme"] = Theme;
            base.OnActionExecuted(context);
        }
    }
}