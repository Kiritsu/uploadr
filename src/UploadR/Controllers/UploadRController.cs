using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace UploadR.Controllers
{
    public class UploadRController : Controller
    {
        protected Guid UserGuid => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
    }
}