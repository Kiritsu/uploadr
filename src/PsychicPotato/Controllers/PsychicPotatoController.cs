using Microsoft.AspNetCore.Mvc;
using PsychicPotato.Database;

namespace PsychicPotato.Controllers
{
    public abstract class PsychicPotatoController : Controller
    {
        protected readonly PsychicPotatoContext _dbContext;

        public PsychicPotatoController(PsychicPotatoContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
