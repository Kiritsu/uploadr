using System;
using Microsoft.AspNetCore.Mvc;
using ShareY.Database;

namespace ShareY.Controllers
{
    public abstract class ShareYController : Controller
    {
        protected readonly ShareYContext _dbContext;

        public ShareYController(ShareYContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
