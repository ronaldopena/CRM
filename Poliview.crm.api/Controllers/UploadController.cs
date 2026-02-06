using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Poliview.crm.services;
using System.Net.Http.Headers;

namespace Poliview.crm.api.Controllers
{
    [Route("upload")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        public class RetornoUpload
        {
            public string? listaIdArquivos { get; set; }
        }

        private IUploadService _service;

        public UploadController(IConfiguration configuration)
        {
            _service = new UploadService(configuration);
            Console.WriteLine("upload controller");
        }
       
        [HttpPost, DisableRequestSizeLimit]
        public IActionResult Upload()
        {
            try
            {
                var ret = _service.SalvarArquivos(Request.Form.Files);                
                var retorno = new RetornoUpload();
                retorno.listaIdArquivos = ret;
                return Ok(retorno);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }            
        }        
    }
}
