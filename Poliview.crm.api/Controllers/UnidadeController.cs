
using Microsoft.AspNetCore.Mvc;
using Poliview.crm.services;

namespace Poliview.crm.api.Controllers
{
    [Route("unidade")]
    [ApiController]
    public class UnidadeController : Controller
    {
        private IUnidadeService _service;

        public UnidadeController(IConfiguration configuration)
        {
            _service = new UnidadeService(configuration);
        }

        [HttpGet("{idempreendimento}/{idbloco}")]
        public IActionResult Listar(int idempreendimento, int idbloco)
        {
            try
            {
                return Ok(_service.Listar(idempreendimento, idbloco));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{idempreendimento}/{idbloco}/relatorio")]
        public IActionResult ListarParaRelatorios(int idempreendimento, int idbloco)
        {
            try
            {
                return Ok(_service.ListarParaRelatorios(idempreendimento, idbloco));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
