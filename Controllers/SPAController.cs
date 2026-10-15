using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace sistema_gastronomico.Controllers
{
    [Authorize] 
    public class SPAController : Controller
    {
        public IActionResult Index()
        {
           
            return View(); 
        }
    }
}