using Microsoft.AspNetCore.Mvc;

namespace PoliviewCrm.CvCrm.Controllers;

public class MensagemController : Controller
{
    [HttpGet]
    [Route("mensagem")]
    public IActionResult Index([FromQuery] string? texto = null)
    {
        ViewBag.Mensagem = TempData["Mensagem"] ?? (object?)texto ?? "";
        return View("Index");
    }
}