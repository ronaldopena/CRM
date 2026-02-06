using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Poliview.crm.services;

namespace Poliview.crm.apicrm.Controllers
{
    public class requisicaoExecSQL
    {
        public string? sql { get; set; }
    }

    [Route("info")]
    [ApiController]
    public class InfoController : Controller
    {
        private IInfoService _service;

        public InfoController(IConfiguration configuration)
        {
            //var connectionString = configuration["conexao"].ToString();
            _service = new InfoService(configuration);
        }

        [HttpGet("")]
        public IActionResult infoCrm()
        {
            return Ok(_service.execute());
        }

        [Authorize]
        [HttpPost("execSQL")]
        public IActionResult ExecSQL(requisicaoExecSQL obj)
        {
            return Ok(_service.execSQL(obj.sql));
        }
    }

}
