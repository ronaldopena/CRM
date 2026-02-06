
using Microsoft.AspNetCore.Mvc;
using Poliview.crm.domain;
using Poliview.crm.models;
using Poliview.crm.services;

namespace Poliview.crm.api.Controllers
{
    [Route("contaemail")]
    [ApiController]
    public class ContaEmailController : Controller
    {
        private IContaEmailService _service;

        public ContaEmailController(IConfiguration configuration)
        {
            _service = new ContaEmailService(configuration);
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

        [HttpGet("{idContaEmail}")]
        public IActionResult ListarPorId(int idContaEmail)
        {
            try
            {
                var retorno = _service.ListarPorId(idContaEmail).Result;
                return Ok(retorno);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("")]
        public async Task<IActionResult> Update(ContaEmail obj)
        {
            try
            {
                Retorno retorno = await _service.Update(obj);
                return Ok(retorno);
            }
            catch (Exception ex)
            {
                var erro = ex.Message;
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("")]
        public async Task<IActionResult> Create(ContaEmail obj)
        {
            try
            {
                Retorno retorno = await _service.Create(obj);
                return Ok(retorno); 
            }
            catch (Exception ex)
            {
                var erro = ex.Message;
                return BadRequest(ex.Message);
            }
        }

    }
}
