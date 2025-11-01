using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DavetLink.Areas.Panel.Controllers
{
    [Area("Panel")]
    [Authorize(Roles = "User")]
    public abstract class BaseController : Controller
    {
    }
}
