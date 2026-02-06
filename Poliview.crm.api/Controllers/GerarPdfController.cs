using Microsoft.AspNetCore.Mvc;
using Poliview.crm.models;
using Poliview.crm.services;

namespace Poliview.crm.api.Controllers
{

    [Route("gerarpdf")]
    [ApiController]
    public class GerarPdfController : Controller
    {
        private IGerarPdfService _service;

        public GerarPdfController(IConfiguration configuration)
        {
            _service = new GerarPdfService(configuration);
        }

        [HttpPost()]
        public IActionResult gerarPdf(GerarPdfRequisicao obj)
        {
            return Ok(_service.listar(obj));
        }
    }
}
