using Microsoft.AspNetCore.Mvc;

namespace DavetLink.Areas.Panel.Controllers
{
    public class HomeController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
