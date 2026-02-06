using Microsoft.AspNetCore.Mvc;
using Poliview.crm.services;

namespace Poliview.crm.apicrm.Controllers
{
    [Route("ocorrencia")]
    [ApiController]
    public class OcorrenciaController : ControllerBase
    {
        private IOcorrenciasAberturaChamadoService _service;

        public OcorrenciaController(IConfiguration configuration)
        {
            _service = new OcorrenciasAberturaChamadoService(configuration);
        }

        [HttpGet()]
        public IActionResult ListarOcorrencias()
        {
            try
            {
                return Ok(_service.RetornaOcorrenciasAberturaChamado());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
