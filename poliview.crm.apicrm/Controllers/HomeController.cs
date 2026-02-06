using Microsoft.AspNetCore.Mvc;

namespace Poliview.crm.apicrm.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
