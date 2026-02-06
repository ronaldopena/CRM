using Microsoft.AspNetCore.Mvc;

namespace Poliview.crm.api.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
