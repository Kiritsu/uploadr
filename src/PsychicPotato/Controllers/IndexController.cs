using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PsychicPotato.Database;
using PsychicPotato.Services;

namespace PsychicPotato.Controllers
{
    [Route("")]
    public class IndexController : PsychicPotatoController
    {
        private readonly FileService _fs;

        public IndexController(PsychicPotatoContext dbContext, FileService fs) : base(dbContext)
        {
            _fs = fs;
        }

        [Route("privacy"), HttpGet]
        public IActionResult Privacy()
        {
            return View();
        }

        [Route(""), HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [Route("{name}"), HttpGet]
        public async Task<IActionResult> GetFile(string name)
        {
            var file = await _fs.TryGetUploadByNameAsync(name);
            if (file.IsSuccess)
            {
                var path = $"./uploads/{file.Value.FileName}";
                var fileBytes = System.IO.File.ReadAllBytes(path);

                return File(fileBytes, file.Value.ContentType);
            }

            return file.Code switch
            {
                1 => Redirect("/"),
                2 => NotFound("File is removed."),
                3 => NotFound("File not found. It has been marked as removed."),
                _ => BadRequest()
            };
        }
    }
}
