using FirebirdSql.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Poliview.crm.services;

namespace Poliview.crm.api.Controllers
{
    public class requisicaoExecSQL
    {
        public string sql { get; set; }
    }

    [Route("info")]
    [ApiController]
    public class InfoController : Controller
    {
        private IInfoService _service;

        public InfoController(IConfiguration configuration)
        {
            _service = new InfoService(configuration);
        }

        [HttpGet("")]
        public IActionResult infoCrm()
        {
            return Ok(_service.execute());
        }

        [HttpPost("execSQL")]
        public IActionResult ExecSQL(requisicaoExecSQL obj)
        {
            return Ok(_service.execSQL(obj.sql));
        }
    }

}
