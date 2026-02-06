using Microsoft.AspNetCore.Mvc;
using Poliview.crm.models;
using Poliview.crm.services;

namespace Poliview.crm.api.Controllers
{

    [Route("boleto")]
    [ApiController]
    public class BoletoController : Controller
    {
        private IBoletoService _service;
        private Serilog.ILogger logApi;

        public BoletoController(IConfiguration configuration)
        {
            _service = new BoletoService(configuration);
        }

        [HttpPost()]
        public IActionResult ListarBoletos(ListarBoletosRequisicao obj)
        {
            Console.WriteLine("Listar Boletos");
            Poliview.crm.infra.Util.ExibirPropriedades(obj);
            // logApi.Debug($"ListarBoletos Controller: {obj.empreendimentosp7} {obj.blocosp7} {obj.unidadesp7}");
            return Ok(_service.Listar(obj).Result);
        }

    }
}
