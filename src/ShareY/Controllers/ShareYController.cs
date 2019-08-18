using System;
using Microsoft.AspNetCore.Mvc;
using ShareY.Extensions;
using ShareY.Models;

namespace ShareY.Controllers
{
    public class ShareYController : Controller
    {
        public BaseViewModel Model => new BaseViewModel { IsAuthenticated = IsAuthenticated, UserToken = UserToken };

        public bool IsAuthenticated => UserToken != null && UserToken != default;
        public Guid UserToken => HttpContext.Session.Get<Guid>("userToken");
    }
}
