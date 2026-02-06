using Microsoft.AspNetCore.Mvc;

namespace Poliview.crm.apicrm.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DownloadController : ControllerBase
    {
        [HttpGet("{fileName}")]
        public IActionResult DownloadFile(string fileName)
        {
            // Substitua pelo caminho da pasta onde o arquivo está armazenado
            var filePath = Path.Combine(@"C:\Poliview\GENCONS430\Website\pdf\", fileName);
            // var filePath = Path.Combine(@"D:\Poliview\CRM\Website\pdf\", fileName);

            // Verifica se o arquivo existe
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            // Lê o arquivo no array de bytes
            var fileBytes = System.IO.File.ReadAllBytes(filePath);

            // Retorna o arquivo para download
            return File(fileBytes, "application/octet-stream", fileName);
        }
    }
}