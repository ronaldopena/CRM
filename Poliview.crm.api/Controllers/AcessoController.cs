using Microsoft.AspNetCore.Mvc;
using Poliview.crm.services;

namespace poliview.crm.api.Controllers
{
    [Route("acesso")]
    [ApiController]
    public class AcessoController : Controller
    {
        private IAcessoService _service;

        public AcessoController(IConfiguration configuration)
        {
            _service = new AcessoService(configuration);
        }

        [HttpPost("{chaveacesso}")]
        public IActionResult Retorna(string chaveacesso)
        {
            Console.WriteLine("Chave acesso");
            try
            {
                return Ok(_service.Listar(chaveacesso));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }            
        }

    }
}
