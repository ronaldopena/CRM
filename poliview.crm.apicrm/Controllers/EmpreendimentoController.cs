
using Microsoft.AspNetCore.Mvc;
using Poliview.crm.domain;
using Poliview.crm.services;

namespace Poliview.crm.apicrm.Controllers
{
    [Route("empreendimento")]
    [ApiController]
    public class EmpreendimentoController : Controller
    {
        private IEmpreendimentoService _service;

        public EmpreendimentoController(IConfiguration configuration)
        {
            _service = new EmpreendimentoService(configuration);
        }

        [HttpGet()]
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

        [HttpGet("relatorio")]
        public IActionResult ListarParaRelatorios()
        {
            try
            {
                return Ok(_service.ListarParaRelatorios());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{idempreendimentosp7}")]
        public IActionResult ListarPorIdSp7(int idempreendimentosp7)
        {
            try
            {
                return Ok(_service.ListaPorIdSp7(idempreendimentosp7));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut()]
        public IActionResult Update(Empreendimento obj)
        {
            try
            {
                Console.WriteLine("CREATE MENSAGEM JSON");
                // var json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                // var json = new JavaScriptSerializer().Serialize(obj);
                // Console.WriteLine(json);
                // Console.WriteLine("id=", obj.id);
                // Console.WriteLine("descricao=", obj.descricao);

                Console.WriteLine("alterar empreendimento");
                return Ok(_service.Update(obj));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
