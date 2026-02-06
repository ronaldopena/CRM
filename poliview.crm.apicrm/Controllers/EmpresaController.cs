using Microsoft.AspNetCore.Mvc;
using Poliview.crm.services;

namespace Poliview.crm.apicrm.Controllers
{
    [Route("empresa")]
    [ApiController]
    public class EmpresaController : Controller
    {
        private IEmpresaService _service;

        public EmpresaController(IConfiguration configuration)
        {
            _service = new EmpresaService(configuration);
        }

        [HttpGet("")]
        public IActionResult Listar()
        {
            try
            {
                return Ok(_service.Listar());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("dominio")]
        public IActionResult ListarPorDominio(string dominio)
        {
            Console.WriteLine("url do dominio");
            Console.WriteLine(dominio);
            try
            {
                return Ok(_service.ListarPorDominio(dominio));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
