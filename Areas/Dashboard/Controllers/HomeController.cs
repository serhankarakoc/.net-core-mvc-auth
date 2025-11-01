using Microsoft.AspNetCore.Mvc;

namespace DavetLink.Areas.Dashboard.Controllers
{
    public class HomeController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
