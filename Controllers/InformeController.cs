using Microsoft.AspNetCore.Mvc;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Models;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Controllers
{
    public class InformeController : Controller
    {
        private readonly RepositorioInforme _repositorioInforme;

        public InformeController(RepositorioInforme repositorioInforme)
        {
            _repositorioInforme = repositorioInforme;
        }

     
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

       
        [HttpGet]
        public IActionResult GetBalance(DateTime desde, DateTime hasta)
        {
            var data = _repositorioInforme.ObtenerBalanceGeneral(desde, hasta.AddDays(1).AddSeconds(-1));
            return Json(data);
        }


        [HttpGet]
        public IActionResult GetPlatosMasVendidos(DateTime desde, DateTime hasta)
        {
            var data = _repositorioInforme.ObtenerPlatosMasVendidos(desde, hasta.AddDays(1).AddSeconds(-1));
            return Json(data);
        }

    
        [HttpGet]
        public IActionResult GetComprasPorProveedor(DateTime desde, DateTime hasta)
        {
            var data = _repositorioInforme.ObtenerComprasPorProveedor(desde, hasta.AddDays(1).AddSeconds(-1));
            return Json(data);
        }

        [HttpGet]
        public IActionResult GetVentasPorMozo(DateTime desde, DateTime hasta)
        {
            var data = _repositorioInforme.ObtenerVentasPorMozo(desde, hasta.AddDays(1).AddSeconds(-1));
            return Json(data);
        }
    }
}