using Microsoft.AspNetCore.Mvc;
using UploadR.Database;

namespace UploadR.Controllers
{
    public abstract class UploadRController : Controller
    {
        protected readonly UploadRContext _dbContext;

        public UploadRController(UploadRContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
