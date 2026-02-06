using Microsoft.AspNetCore.Mvc;
using Poliview.crm.models;
using Poliview.crm.services;

namespace Poliview.crm.api.Controllers
{
    [Route("historico")]
    [ApiController]
    public class HistoricoController : ControllerBase
    {
        private IHistoricoService _historicoService;

        public HistoricoController(IConfiguration configuration)
        {
            _historicoService = new HistoricoService(configuration);
            Console.WriteLine("HistoricoController");
        }
              

        [HttpPost()]
        public IActionResult Incluir(HistoricoChamadosIncluirRequisicao obj)
        {
            Console.WriteLine("Incluir Historico " + obj);
            try
            {
                return Ok(_historicoService.Incluir(obj));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
