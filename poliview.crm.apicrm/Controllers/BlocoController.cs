
using Microsoft.AspNetCore.Mvc;
using Poliview.crm.services;

namespace Poliview.crm.apicrm.Controllers
{
    [Route("bloco")]
    [ApiController]
    public class BlocoController : Controller
    {
        private IBlocoService _service;

        public BlocoController(IConfiguration configuration)
        {
            _service = new BlocoService(configuration);
        }

        [HttpGet("{idempreendimento}")]
        public IActionResult Listar(int idempreendimento)
        {
            try
            {
                return Ok(_service.Listar(idempreendimento));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
